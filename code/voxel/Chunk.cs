using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Chunk : IValid
	{
		public struct SliceUpdate : IEquatable<SliceUpdate>
		{
			public IntVector3 Position;
			public int Direction;

			#region Equality
			public static bool operator ==( SliceUpdate left, SliceUpdate right ) => left.Equals( right );
			public static bool operator !=( SliceUpdate left, SliceUpdate right ) => !(left == right);
			public override bool Equals( object obj ) => obj is SliceUpdate o && Equals( o );
			public bool Equals( SliceUpdate o ) => Position == o.Position && Direction == o.Direction;
			public override int GetHashCode() => HashCode.Combine( Position.x, Position.y, Position.z, Direction );
			#endregion
		}

		private struct BlockFaceData
		{
			public int BlockIndex;
			public bool Culled;
			public byte Type;
			public byte Side;

			public bool Equals( BlockFaceData face )
			{
				return face.Culled == Culled && face.Type == Type;
			}
		};

		public static readonly int ChunkSize = 32;
		public static readonly int VoxelSize = 48;

		public Dictionary<IntVector3, BlockData> Data { get; set; } = new();
		public HashSet<SliceUpdate> PendingSliceUpdates { get; set; } = new();
		public HashSet<IntVector3> DirtyData { get; set; } = new();
		public bool Initialized { get; private set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public ChunkLightMap LightMap { get; set; }
		public byte[] Blocks;
		public IntVector3 Offset;
		public int Index;
		public Map Map;

		[ThreadStatic]
		private static BlockFaceData[] ThreadStaticBlockFaceMask;

		private static BlockFaceData[] GetBlockFaceMask()
		{
			return ThreadStaticBlockFaceMask ??= new BlockFaceData[ChunkSize * ChunkSize * ChunkSize];
		}

		private readonly ChunkSlice[] Slices = new ChunkSlice[ChunkSize * 6];
		private Dictionary<int, BlockEntity> Entities { get; set; }
		private SceneObject TranslucentSceneObject { get; set; }
		private SceneObject OpaqueSceneObject { get; set; }
		private Model TranslucentModel { get; set; }
		private Model OpaqueModel { get; set; }
		private Mesh TranslucentMesh { get; set; }
		private Mesh OpaqueMesh { get; set; }

		public bool IsValid => true;

		public Chunk() { }

		public Chunk( Map map, int x, int y, int z )
		{
			Blocks = new byte[ChunkSize * ChunkSize * ChunkSize];
			Entities = new();
			LightMap = new ChunkLightMap( this, map );
			Offset = new IntVector3( x * ChunkSize, y * ChunkSize, z * ChunkSize );
			Index = x + y * map.NumChunksX + z * map.NumChunksX * map.NumChunksY;
			Map = map;
		}

		public async Task Init()
		{
			if ( Initialized )
				return;

			await GameTask.RunInThreadAsync( PropagateSunlight );
			await GameTask.RunInThreadAsync( PerformFullTorchUpdate );

			LightMap.Update();

			await GameTask.RunInThreadAsync( UpdateBlockSlices );

			CreateEntities();

			Initialized = true;

			foreach ( var update in PendingSliceUpdates )
			{
				UpdateBlockSlice( update.Position, update.Direction );
			}

			PendingSliceUpdates.Clear();

			var translucentModelBuilder = new ModelBuilder();
			var opaqueModelBuilder = new ModelBuilder();

			if ( IsClient )
			{
				var material = Material.Load( "materials/corewars/voxel.vmat" );
				TranslucentMesh = new Mesh( material );
				OpaqueMesh = new Mesh( material );

				var boundsMin = Vector3.Zero;
				var boundsMax = boundsMin + (ChunkSize * VoxelSize);
				TranslucentMesh.SetBounds( boundsMin, boundsMax );
				OpaqueMesh.SetBounds( boundsMin, boundsMax );
			}

			Build();

			if ( IsClient )
			{
				translucentModelBuilder.AddMesh( TranslucentMesh );
				opaqueModelBuilder.AddMesh( OpaqueMesh );
			}

			TranslucentModel = translucentModelBuilder.Create();
			OpaqueModel = opaqueModelBuilder.Create();

			if ( IsClient )
			{
				var transform = new Transform( Offset * (float)VoxelSize );

				OpaqueSceneObject = new SceneObject( OpaqueModel, transform );
				OpaqueSceneObject.SetValue( "VoxelSize", VoxelSize );
				OpaqueSceneObject.SetValue( "LightMap", LightMap.Texture );

				TranslucentSceneObject = new SceneObject( TranslucentModel, transform );
				TranslucentSceneObject.SetValue( "VoxelSize", VoxelSize );
				TranslucentSceneObject.SetValue( "LightMap", LightMap.Texture );
			}

			Event.Register( this );

			if ( IsClient )
			{
				UpdateAdjacents( true );
			}
		}

		public void PerformFullTorchUpdate()
		{
			for ( var x = 0; x < ChunkSize; x++ )
			{
				for ( var y = 0; y < ChunkSize; y++ )
				{
					for ( var z = 0; z < ChunkSize; z++ )
					{
						var position = new IntVector3( x, y, z );
						var blockIndex = GetLocalPositionIndex( position );
						var block = Map.GetBlockType( Blocks[blockIndex] );

						if ( block.LightLevel.x > 0 || block.LightLevel.y > 0 || block.LightLevel.z > 0 )
						{
							LightMap.AddRedTorchLight( position, (byte)block.LightLevel.x );
							LightMap.AddGreenTorchLight( position, (byte)block.LightLevel.y );
							LightMap.AddBlueTorchLight( position, (byte)block.LightLevel.z );
						}
					}
				}
			}
		}

		public async void FullUpdate()
		{
			await GameTask.RunInThreadAsync( UpdateBlockSlices );
			Build();
		}

		public void UpdateAdjacents( bool recurseNeighbours = false )
		{
			var currentOffset = Offset;
			currentOffset.x--;

			var westChunk = Map.GetChunkIndex( currentOffset );
			currentOffset.x++;
			currentOffset.y--;

			var southChunk = Map.GetChunkIndex( currentOffset );
			currentOffset.y++;
			currentOffset.y += ChunkSize + 1;

			var northChunk = Map.GetChunkIndex( currentOffset );
			currentOffset.y -= ChunkSize + 1;
			currentOffset.x += ChunkSize + 1;

			var eastChunk = Map.GetChunkIndex( currentOffset );
			currentOffset.x -= ChunkSize + 1;
			currentOffset.z += ChunkSize + 1;

			var topChunk = Map.GetChunkIndex( currentOffset );
			currentOffset.z -= ChunkSize + 1;
			currentOffset.z--;

			var bottomChunk = Map.GetChunkIndex( currentOffset );

			UpdateNeighbourLightMap( "LightMapWest", westChunk, recurseNeighbours );
			UpdateNeighbourLightMap( "LightMapEast", eastChunk, recurseNeighbours );
			UpdateNeighbourLightMap( "LightMapNorth", northChunk, recurseNeighbours );
			UpdateNeighbourLightMap( "LightMapSouth", southChunk, recurseNeighbours );
			UpdateNeighbourLightMap( "LightMapTop", topChunk, recurseNeighbours );
			UpdateNeighbourLightMap( "LightMapBottom", bottomChunk, recurseNeighbours );
		}

		public void Build()
		{
			if ( IsServer )
			{
				BuildCollision();
				return;
			}

			BuildMeshAndCollision();
		}

		public void DeserializeData( byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					while ( stream.Position < stream.Length )
					{
						var x = reader.ReadByte();
						var y = reader.ReadByte();
						var z = reader.ReadByte();
						var blockIndex = GetLocalPositionIndex( x, y, z );
						var blockId = Blocks[blockIndex];
						var block = Map.GetBlockType( blockId );
						var position = new IntVector3( x, y, z );

						if ( !Data.TryGetValue( position, out var blockData ) )
						{
							blockData = block.CreateDataInstance();
							blockData.Chunk = this;
							blockData.LocalPosition = position;
							Data.Add( position, blockData );
						}

						blockData.Deserialize( reader );
					}
				}
			}
		}

		public byte[] SerializeData()
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					foreach ( var kv in Data )
					{
						var position = kv.Key;
						writer.Write( (byte)position.x );
						writer.Write( (byte)position.y );
						writer.Write( (byte)position.z );
						kv.Value.Serialize( writer );
					}

					return stream.ToArray();
				}
			}
		}

		public T GetOrCreateData<T>( IntVector3 position ) where T : BlockData
		{
			if ( Data.TryGetValue( position, out var data ) )
				return data as T;

			var blockId = GetLocalPositionBlock( position );
			var block = Map.Current.GetBlockType( blockId );

			data = block.CreateDataInstance();
			data.Chunk = this;
			data.LocalPosition = position;
			Data.Add( position, data );

			data.IsDirty = true;

			return data as T;
		}

		public T GetData<T>( IntVector3 position ) where T : BlockData
		{
			if ( Data.TryGetValue( position, out var data ) )
				return data as T;
			else
				return null;
		}

		public int GetLocalPositionIndex( int x, int y, int z )
		{
			return x + y * ChunkSize + z * ChunkSize * ChunkSize;
		}

		public int GetLocalPositionIndex( IntVector3 position )
		{
			return position.x + position.y * ChunkSize + position.z * ChunkSize * ChunkSize;
		}

		public byte GetMapPositionBlock( IntVector3 position )
		{
			var x = position.x % ChunkSize;
			var y = position.y % ChunkSize;
			var z = position.z % ChunkSize;
			var index = x + y * ChunkSize + z * ChunkSize * ChunkSize;
			return Blocks[index];
		}

		public byte GetLocalPositionBlock( int x, int y, int z )
		{
			return Blocks[GetLocalPositionIndex( x, y, z )];
		}

		public byte GetLocalPositionBlock( IntVector3 position )
		{
			return Blocks[GetLocalPositionIndex( position )];
		}

		public byte GetLocalIndexBlock( int index )
		{
			return Blocks[index];
		}

		public IntVector3 ToMapPosition( IntVector3 position )
		{
			return Offset + position;
		}

		public void CreateEntities()
		{
			for ( var x = 0; x < ChunkSize; x++ )
			{
				for ( var y = 0; y < ChunkSize; y++ )
				{
					for ( var z = 0; z < ChunkSize; z++ )
					{
						var position = new IntVector3( x, y, z );
						var blockId = GetLocalPositionBlock( position );
						var block = Map.GetBlockType( blockId );
						var entityName = IsServer ? block.ServerEntity : block.ClientEntity;

						if ( !string.IsNullOrEmpty( entityName ) )
						{
							var entity = Library.Create<BlockEntity>( entityName );
							entity.BlockType = block;
							SetEntity( position, entity );
						}
					}
				}
			}
		}

		public void PropagateSunlight()
		{
			var z = ChunkSize - 1;

			for ( var x = 0; x < ChunkSize; x++ )
			{
				for ( var y = 0; y < ChunkSize; y++ )
				{
					var position = new IntVector3( x, y, z );
					var blockId = GetLocalPositionBlock( position );
					var block = Map.GetBlockType( blockId );

					if ( block.IsTranslucent )
					{
						LightMap.AddSunLight( position, 15 );
					}
				}
			}

			var chunkAbove = GetNeighbour( BlockFace.Top );
			if ( !chunkAbove.IsValid() ) return;
			if ( !chunkAbove.Initialized ) return;

			for ( var x = 0; x < ChunkSize; x++ )
			{
				for ( var y = 0; y < ChunkSize; y++ )
				{
					var lightLevel = Map.GetSunLight( chunkAbove.Offset + new IntVector3( x, y, 0 ) );

					if ( lightLevel > 0 )
					{
						LightMap.AddSunLight( new IntVector3( x, y, ChunkSize - 1 ), lightLevel );
					}
				}
			}
		}

		public Chunk GetNeighbour( BlockFace direction )
		{
			var directionIndex = (int)direction;
			var neighbourPosition = Offset + (BlockDirections[directionIndex] * ChunkSize);

			if ( Map.IsInside( neighbourPosition ) )
			{
				var neighbourIndex = Map.GetChunkIndex( neighbourPosition );
				var neighbour = Map.Chunks[neighbourIndex];

				return neighbour;
			}

			return null;
		}

		public void SetBlock( IntVector3 position, byte blockId )
		{
			Blocks[GetLocalPositionIndex( position )] = blockId;
		}

		public void SetBlock( int index, byte blockId )
		{
			Blocks[index] = blockId;
		}

		public void Destroy()
		{
			if ( TranslucentSceneObject != null )
			{
				TranslucentSceneObject.Delete();
				TranslucentSceneObject = null;
			}

			if ( OpaqueSceneObject != null )
			{
				OpaqueSceneObject.Delete();
				OpaqueSceneObject = null;
			}

			foreach ( var kv in Entities )
			{
				kv.Value.Delete();
			}

			Entities.Clear();

			foreach ( var slice in Slices )
			{
				if ( slice == null )
					continue;

				slice.Body = null;
			}

			Event.Unregister( this );
		}

		public Entity GetEntity( IntVector3 position )
		{
			var index = GetLocalPositionIndex( position );
			if ( Entities.TryGetValue( index, out var entity ) )
				return entity;
			else
				return null;
		}

		public void SetEntity( IntVector3 position, BlockEntity entity )
		{
			var mapPosition = Offset + position;

			entity.Map = Map;
			entity.Chunk = this;
			entity.BlockPosition = mapPosition;
			entity.LocalBlockPosition = position;
			entity.CenterOnBlock( true, false );
			entity.Initialize();

			var index = GetLocalPositionIndex( position );
			RemoveEntity( position );
			Entities.Add( index, entity );
		}

		public void RemoveEntity( IntVector3 position )
		{
			var index = GetLocalPositionIndex( position );
			if ( Entities.TryGetValue( index, out var entity ) )
			{
				entity.Delete();
				Entities.Remove( index );
			}
		}

		public void BuildMeshAndCollision()
		{
			if ( !OpaqueMesh.IsValid || !TranslucentMesh.IsValid )
				return;

			int translucentVertexCount = 0;
			int opaqueVertexCount = 0;

			foreach ( var slice in Slices )
			{
				translucentVertexCount += slice.TranslucentVertices.Count;
				opaqueVertexCount += slice.OpaqueVertices.Count;
			}

			if ( TranslucentMesh.HasVertexBuffer )
				TranslucentMesh.SetVertexBufferSize( translucentVertexCount );
			else
				TranslucentMesh.CreateVertexBuffer<BlockVertex>( Math.Max( 1, translucentVertexCount ), BlockVertex.Layout );

			if ( OpaqueMesh.HasVertexBuffer )
				OpaqueMesh.SetVertexBufferSize( opaqueVertexCount );
			else
				OpaqueMesh.CreateVertexBuffer<BlockVertex>( Math.Max( 1, opaqueVertexCount ), BlockVertex.Layout );

			translucentVertexCount = 0;
			opaqueVertexCount = 0;

			foreach ( var slice in Slices )
			{
				if ( slice.IsDirty )
				{
					if ( slice.Shape != null )
					{
						slice.Body.RemoveShape( slice.Shape, false );
						slice.Shape = null;
					}

					if ( slice.CollisionVertices.Count > 0 && slice.CollisionIndices.Count > 0 )
					{
						slice.Shape = slice.Body.AddMeshShape( slice.CollisionVertices.ToArray(), slice.CollisionIndices.ToArray() );
					}
				}

				slice.IsDirty = false;

				if ( slice.OpaqueVertices.Count > 0 )
				{
					OpaqueMesh.SetVertexBufferData( slice.OpaqueVertices, opaqueVertexCount );
					opaqueVertexCount += slice.OpaqueVertices.Count;
				}

				if ( slice.TranslucentVertices.Count > 0 )
				{
					TranslucentMesh.SetVertexBufferData( slice.TranslucentVertices, translucentVertexCount );
					translucentVertexCount += slice.TranslucentVertices.Count;
				}
			}

			OpaqueMesh.SetVertexRange( 0, opaqueVertexCount );
			TranslucentMesh.SetVertexRange( 0, translucentVertexCount );
		}

		private void UpdateNeighbourLightMap( string name, int chunkIndex, bool recurseNeighbours = false )
		{
			if ( chunkIndex >= 0 && chunkIndex < Map.Chunks.Length )
			{
				var neighbour = Map.Chunks[chunkIndex];

				if ( neighbour != null && neighbour.Initialized )
				{
					TranslucentSceneObject.SetValue( name, neighbour.LightMap.Texture );
					OpaqueSceneObject.SetValue( name, neighbour.LightMap.Texture );
					if ( recurseNeighbours ) neighbour.UpdateAdjacents();
				}
			}
		}

		private void BuildCollision()
		{
			foreach ( var slice in Slices )
			{
				if ( slice.IsDirty && slice.Body.IsValid() )
				{
					if ( slice.Shape != null )
					{
						slice.Body.RemoveShape( slice.Shape, false );
						slice.Shape = null;
					}

					if ( slice.CollisionVertices.Count > 0 && slice.CollisionIndices.Count > 0 )
					{
						slice.Shape = slice.Body.AddMeshShape( slice.CollisionVertices.ToArray(), slice.CollisionIndices.ToArray() );
					}
				}

				slice.IsDirty = false;
			}
		}

		static readonly IntVector3[] BlockVertices = new[]
		{
			new IntVector3( 0, 0, 1 ),
			new IntVector3( 0, 1, 1 ),
			new IntVector3( 1, 1, 1 ),
			new IntVector3( 1, 0, 1 ),
			new IntVector3( 0, 0, 0 ),
			new IntVector3( 0, 1, 0 ),
			new IntVector3( 1, 1, 0 ),
			new IntVector3( 1, 0, 0 ),
		};

		static readonly int[] BlockIndices = new[]
		{
			2, 1, 0, 0, 3, 2,
			5, 6, 7, 7, 4, 5,
			4, 7, 3, 3, 0, 4,
			6, 5, 1, 1, 2, 6,
			5, 4, 0, 0, 1, 5,
			7, 6, 2, 2, 3, 7,
		};

		public static readonly IntVector3[] BlockDirections = new[]
		{
			new IntVector3( 0, 0, 1 ),
			new IntVector3( 0, 0, -1 ),
			new IntVector3( 0, -1, 0 ),
			new IntVector3( 0, 1, 0 ),
			new IntVector3( -1, 0, 0 ),
			new IntVector3( 1, 0, 0 ),
		};

		static readonly int[] BlockDirectionAxis = new[]
		{
			2, 2, 1, 1, 0, 0
		};

		private void AddQuad( ChunkSlice slice, int x, int y, int z, int width, int height, int widthAxis, int heightAxis, int face, byte blockId )
		{
			var block = Map.GetBlockType( blockId );

			if ( block == null )
				throw new Exception( $"Unable to find a block type registered with the id: {blockId}!" );

			var textureId = block.GetTextureId( (BlockFace)face, this, x, y, z );
			var normal = (byte)face;
			var faceData = (uint)((textureId & 31) << 18 | (0 & 15) << 23 | (normal & 7) << 27);
			var collisionIndex = slice.CollisionIndices.Count;

			for ( int i = 0; i < 6; ++i )
			{
				int vi = BlockIndices[(face * 6) + i];
				var vOffset = BlockVertices[vi];

				vOffset[widthAxis] *= width;
				vOffset[heightAxis] *= height;

				var vertex = new BlockVertex( (uint)(x + vOffset.x), (uint)(y + vOffset.y), (uint)(z + vOffset.z), (uint)x, (uint)y, (uint)z, faceData );

				if ( block.IsTranslucent )
					slice.TranslucentVertices.Add( vertex );
				else
					slice.OpaqueVertices.Add( vertex );

				if ( !block.IsPassable )
				{
					slice.CollisionVertices.Add( new Vector3( (x + vOffset.x) + Offset.x, (y + vOffset.y) + Offset.y, (z + vOffset.z) + Offset.z ) * VoxelSize );
					slice.CollisionIndices.Add( collisionIndex + i );
				}
			}
		}

		BlockFaceData GetBlockFace( IntVector3 position, int side )
		{
			var p = Offset + position;
			var blockEmpty = Map.IsEmpty( p );
			var blockId = blockEmpty ? (byte)0 : Map.GetBlock( p );
			var block = Map.GetBlockType( blockId );

			var face = new BlockFaceData
			{
				BlockIndex = GetLocalPositionIndex( position ),
				Side = (byte)side,
				Culled = !block.HasTexture,
				Type = blockId,
			};

			var adjacentBlockPosition = Map.GetAdjacentPosition( p, side );
			var adjacentBlockId = Map.GetBlock( adjacentBlockPosition );
			var adjacentBlock = Map.GetBlockType( adjacentBlockId );

			if ( !block.IsTranslucent && !face.Culled && (adjacentBlock != null && !adjacentBlock.IsTranslucent) )
				face.Culled = true;

			return face;
		}

		static int GetSliceIndex( int position, int direction )
		{
			int sliceIndex = 0;

			for ( int i = 0; i < direction; ++i )
			{
				sliceIndex += ChunkSize;
			}

			sliceIndex += position;

			return sliceIndex;
		}

		public void UpdateBlockSlice( IntVector3 position, int direction )
		{
			if ( !Initialized )
			{
				PendingSliceUpdates.Add( new SliceUpdate
				{
					Position = position,
					Direction = direction
				} );

				return;
			}

			var blockFaceMask = GetBlockFaceMask();
			int vertexOffset = 0;
			int axis = BlockDirectionAxis[direction];
			int sliceIndex = GetSliceIndex( position[axis], direction );
			var slice = Slices[sliceIndex];

			if ( slice.IsDirty ) return;

			slice.IsDirty = true;
			slice.OpaqueVertices.Clear();
			slice.TranslucentVertices.Clear();
			slice.CollisionVertices.Clear();
			slice.CollisionIndices.Clear();

			BlockFaceData faceA;
			BlockFaceData faceB;

			int uAxis = (axis + 1) % 3;
			int vAxis = (axis + 2) % 3;
			int faceSide = direction;

			var blockPosition = new IntVector3( 0, 0, 0 );
			blockPosition[axis] = position[axis];
			var blockOffset = BlockDirections[direction];

			bool maskEmpty = true;

			int n = 0;

			for ( blockPosition[vAxis] = 0; blockPosition[vAxis] < ChunkSize; blockPosition[vAxis]++ )
			{
				for ( blockPosition[uAxis] = 0; blockPosition[uAxis] < ChunkSize; blockPosition[uAxis]++ )
				{
					faceB = new()
					{
						Culled = true,
						Side = (byte)faceSide,
						Type = 0,
					};

					faceA = GetBlockFace( blockPosition, faceSide );

					if ( (blockPosition[axis] + blockOffset[axis]) < ChunkSize )
					{
						faceB = GetBlockFace( blockPosition + blockOffset, faceSide );
					}

					if ( !faceA.Culled && !faceB.Culled && faceA.Equals( faceB ) )
					{
						blockFaceMask[n].Culled = true;
					}
					else
					{
						blockFaceMask[n] = faceA;

						if ( !faceA.Culled )
						{
							maskEmpty = false;
						}
					}

					n++;
				}
			}

			if ( maskEmpty ) return;

			n = 0;

			for ( int j = 0; j < ChunkSize; j++ )
			{
				for ( int i = 0; i < ChunkSize; )
				{
					if ( blockFaceMask[n].Culled )
					{
						i++;
						n++;

						continue;
					}

					int faceWidth;
					int faceHeight;

					if ( Map.GreedyMeshing )
					{
						for ( faceWidth = 1; i + faceWidth < ChunkSize && !blockFaceMask[n + faceWidth].Culled && blockFaceMask[n + faceWidth].Equals( blockFaceMask[n] ); faceWidth++ )
						{

						}

						bool done = false;

						for ( faceHeight = 1; j + faceHeight < ChunkSize; faceHeight++ )
						{
							for ( int k = 0; k < faceWidth; k++ )
							{
								var maskFace = blockFaceMask[n + k + faceHeight * ChunkSize];

								if ( maskFace.Culled || !maskFace.Equals( blockFaceMask[n] ) )
								{
									done = true;
									break;
								}
							}

							if ( done ) break;
						}
					}
					else
					{
						faceWidth = 1;
						faceHeight = 1;
					}

					if ( !blockFaceMask[n].Culled )
					{
						blockPosition[uAxis] = i;
						blockPosition[vAxis] = j;

						AddQuad( slice,
							blockPosition.x, blockPosition.y, blockPosition.z,
							faceWidth, faceHeight, uAxis, vAxis,
							blockFaceMask[n].Side, blockFaceMask[n].Type );

						vertexOffset += 6;
					}

					for ( int l = 0; l < faceHeight; ++l )
					{
						for ( int k = 0; k < faceWidth; ++k )
						{
							blockFaceMask[n + k + l * ChunkSize].Culled = true;
						}
					}

					i += faceWidth;
					n += faceWidth;
				}
			}
		}

		public void UpdateBlockSlices()
		{
			var blockFaceMask = GetBlockFaceMask();

			for ( int i = 0; i < Slices.Length; ++i )
			{
				Slices[i] = new ChunkSlice();
			}

			IntVector3 blockPosition;
			IntVector3 blockOffset;

			BlockFaceData faceA;
			BlockFaceData faceB;

			for ( int faceSide = 0; faceSide < 6; faceSide++ )
			{
				int axis = BlockDirectionAxis[faceSide];
				int uAxis = (axis + 1) % 3;
				int vAxis = (axis + 2) % 3;

				blockPosition = new IntVector3( 0, 0, 0 );
				blockOffset = BlockDirections[faceSide];

				for ( blockPosition[axis] = 0; blockPosition[axis] < ChunkSize; blockPosition[axis]++ )
				{
					var n = 0;
					var maskEmpty = true;
					var sliceIndex = GetSliceIndex( blockPosition[axis], faceSide );
					var slice = Slices[sliceIndex];

					slice.IsDirty = true;
					slice.OpaqueVertices.Clear();
					slice.TranslucentVertices.Clear();
					slice.CollisionVertices.Clear();
					slice.CollisionIndices.Clear();

					for ( blockPosition[vAxis] = 0; blockPosition[vAxis] < ChunkSize; blockPosition[vAxis]++ )
					{
						for ( blockPosition[uAxis] = 0; blockPosition[uAxis] < ChunkSize; blockPosition[uAxis]++ )
						{
							faceB = new()
							{
								Culled = true,
								Side = (byte)faceSide,
								Type = 0,
							};

							faceA = GetBlockFace( blockPosition, faceSide );

							if ( (blockPosition[axis] + blockOffset[axis]) < ChunkSize )
							{
								faceB = GetBlockFace( blockPosition + blockOffset, faceSide );
							}

							if ( !faceA.Culled && !faceB.Culled && faceA.Equals( faceB ) )
							{
								blockFaceMask[n].Culled = true;
							}
							else
							{
								blockFaceMask[n] = faceA;

								if ( !faceA.Culled )
								{
									maskEmpty = false;
								}
							}

							n++;
						}
					}

					if ( maskEmpty )
					{
						continue;
					}

					n = 0;

					for ( int j = 0; j < ChunkSize; j++ )
					{
						for ( int i = 0; i < ChunkSize; )
						{
							if ( blockFaceMask[n].Culled )
							{
								i++;
								n++;

								continue;
							}

							int faceWidth;
							int faceHeight;

							if ( Map.GreedyMeshing )
							{
								for ( faceWidth = 1; i + faceWidth < ChunkSize && !blockFaceMask[n + faceWidth].Culled && blockFaceMask[n + faceWidth].Equals( blockFaceMask[n] ); faceWidth++ )
								{

								}

								bool done = false;

								for ( faceHeight = 1; j + faceHeight < ChunkSize; faceHeight++ )
								{
									for ( int k = 0; k < faceWidth; k++ )
									{
										var maskFace = blockFaceMask[n + k + faceHeight * ChunkSize];

										if ( maskFace.Culled || !maskFace.Equals( blockFaceMask[n] ) )
										{
											done = true;
											break;
										}
									}

									if ( done ) break;
								}
							}
							else
							{
								faceWidth = 1;
								faceHeight = 1;
							}

							if ( !blockFaceMask[n].Culled )
							{
								blockPosition[uAxis] = i;
								blockPosition[vAxis] = j;

								AddQuad( slice,
									blockPosition.x, blockPosition.y, blockPosition.z,
									faceWidth, faceHeight, uAxis, vAxis,
									blockFaceMask[n].Side, blockFaceMask[n].Type );
							}

							for ( int l = 0; l < faceHeight; ++l )
							{
								for ( int k = 0; k < faceWidth; ++k )
								{
									blockFaceMask[n + k + l * ChunkSize].Culled = true;
								}
							}

							i += faceWidth;
							n += faceWidth;
						}
					}
				}
			}
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( DirtyData.Count > 0 )
			{
				using ( var stream = new MemoryStream() )
				{
					using ( var writer = new BinaryWriter( stream ) )
					{
						foreach ( var position in DirtyData )
						{
							var data = GetData<BlockData>( position );
							if ( data == null ) continue;
							writer.Write( (byte)position.x );
							writer.Write( (byte)position.y );
							writer.Write( (byte)position.z );
							data.Serialize( writer );
							data.IsDirty = false;
						}

						Map.ReceiveDataUpdate( To.Everyone, Index, stream.ToArray() );
					}
				}
			}

			DirtyData.Clear();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			LightMap.Update();
		}
	}
}
