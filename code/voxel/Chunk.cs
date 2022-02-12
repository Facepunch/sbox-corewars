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
			public bool IsTranslucent;
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
		public bool QueueFullUpdate { get; set; }
		public bool QueueRebuild { get; set; }
		public bool IsModelCreated { get; private set; }
		public bool Initialized { get; private set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public ChunkLightMap LightMap { get; set; }
		public byte[] Blocks;
		public IntVector3 Offset;
		public int Index;
		public Map Map;

		public List<BlockVertex> TranslucentVertices = new();
		public List<BlockVertex> OpaqueVertices = new();
		public List<Vector3> CollisionVertices = new();
		public List<int> CollisionIndices = new();
		public PhysicsBody Body;
		public PhysicsShape Shape;

		[ThreadStatic]
		private static BlockFaceData[] ThreadStaticBlockFaceMask;

		private static BlockFaceData[] GetBlockFaceMask()
		{
			return ThreadStaticBlockFaceMask ??= new BlockFaceData[ChunkSize * ChunkSize * ChunkSize];
		}

		private Dictionary<int, BlockEntity> Entities { get; set; }
		private SceneObject TranslucentSceneObject { get; set; }
		private SceneObject OpaqueSceneObject { get; set; }
		private ModelBuilder TranslucentModelBuilder { get; set; }
		private Model TranslucentModel { get; set; }
		private ModelBuilder OpaqueModelBuilder { get; set; }
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
			Body = PhysicsWorld.WorldBody;
			Map = map;
		}

		public async Task Init()
		{
			if ( Initialized )
				return;

			if ( IsClient )
			{
				await GameTask.RunInThreadAsync( PropagateSunlight );
				await GameTask.RunInThreadAsync( PerformFullTorchUpdate );
				LightMap.Update();
			}

			CreateEntities();
			Initialized = true;

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

			QueueFullUpdate = true;

			Event.Register( this );

			if ( IsClient )
			{
				QueueNeighbourFullUpdate();
			}
		}

		public Voxel GetVoxel( IntVector3 position )
		{
			return GetVoxel( position.x, position.y, position.z );
		}

		public Voxel GetVoxel( int x, int y, int z )
		{
			var voxel = new Voxel();
			voxel.LocalPosition = new IntVector3( x, y, z );
			voxel.Position = Offset + voxel.LocalPosition;
			voxel.BlockIndex = GetLocalPositionIndex( x, y, z );
			voxel.ChunkIndex = Index;
			voxel.BlockId = GetLocalIndexBlock( voxel.BlockIndex );
			voxel.IsValid = true;
			return voxel;
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

		public void QueueNeighbourFullUpdate()
		{
			QueueNeighbourFullUpdate( BlockFace.Top );
			QueueNeighbourFullUpdate( BlockFace.Bottom );
			QueueNeighbourFullUpdate( BlockFace.North );
			QueueNeighbourFullUpdate( BlockFace.East );
			QueueNeighbourFullUpdate( BlockFace.South );
			QueueNeighbourFullUpdate( BlockFace.West );
		}

		public IntVector3 GetAdjacentChunkCenter( BlockFace direction )
		{
			IntVector3 position;

			if ( direction == BlockFace.Top )
				position = new IntVector3( ChunkSize / 2, ChunkSize / 2, ChunkSize + 1 );
			else if ( direction == BlockFace.Bottom )
				position = new IntVector3( ChunkSize / 2, ChunkSize / 2, -1 );
			else if ( direction == BlockFace.North )
				position = new IntVector3( ChunkSize / 2, ChunkSize + 1, ChunkSize / 2 );
			else if ( direction == BlockFace.South )
				position = new IntVector3( ChunkSize / 2, -1, ChunkSize / 2 );
			else if ( direction == BlockFace.East )
				position = new IntVector3( ChunkSize + 1, ChunkSize / 2, ChunkSize / 2 );
			else
				position = new IntVector3( -1, ChunkSize / 2, ChunkSize / 2 );

			return Offset + position;
		}

		public void QueueNeighbourFullUpdate( BlockFace direction )
		{
			var neighbour = GetNeighbour( direction );

			if ( neighbour.IsValid() && neighbour.Initialized )
			{
				neighbour.QueueFullUpdate = true;
			}
		}

		public void Build()
		{
			if ( IsServer )
			{
				BuildCollision();
				return;
			}

			BuildMeshAndCollision();

			if ( !IsModelCreated )
			{
				TranslucentModelBuilder = new ModelBuilder();
				OpaqueModelBuilder = new ModelBuilder();

				TranslucentModelBuilder.AddMesh( TranslucentMesh );
				OpaqueModelBuilder.AddMesh( OpaqueMesh );

				TranslucentModel = TranslucentModelBuilder.Create();
				OpaqueModel = OpaqueModelBuilder.Create();

				var transform = new Transform( Offset * (float)VoxelSize );

				OpaqueSceneObject = new SceneObject( OpaqueModel, transform );
				OpaqueSceneObject.SetValue( "VoxelSize", VoxelSize );
				OpaqueSceneObject.SetValue( "LightMap", LightMap.Texture );

				TranslucentSceneObject = new SceneObject( TranslucentModel, transform );
				TranslucentSceneObject.SetValue( "VoxelSize", VoxelSize );
				TranslucentSceneObject.SetValue( "LightMap", LightMap.Texture );

				UpdateAdjacents( true );

				IsModelCreated = true;
			}
		}

		public void DeserializeData( BinaryReader reader )
		{
			var count = reader.ReadInt32();

			for ( var i = 0; i < count; i++ )
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

		public void DeserializeData( byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					DeserializeData( reader );
				}
			}
		}

		public void SerializeData( BinaryWriter writer )
		{
			writer.Write( Data.Count );

			foreach ( var kv in Data )
			{
				var position = kv.Key;
				writer.Write( (byte)position.x );
				writer.Write( (byte)position.y );
				writer.Write( (byte)position.z );
				kv.Value.Serialize( writer );
			}
		}

		public byte[] SerializeData()
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					SerializeData( writer );
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
			Body = null;

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

			int translucentVertexCount = TranslucentVertices.Count;
			int opaqueVertexCount = OpaqueVertices.Count;

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

			if ( Shape != null )
			{
				Body.RemoveShape( Shape, false );
				Shape = null;
			}

			if ( CollisionVertices.Count > 0 && CollisionIndices.Count > 0 )
			{
				Shape = Body.AddMeshShape( CollisionVertices.ToArray(), CollisionIndices.ToArray() );
			}

			if ( OpaqueVertices.Count > 0 )
			{
				OpaqueMesh.SetVertexBufferData( OpaqueVertices, opaqueVertexCount );
				opaqueVertexCount += OpaqueVertices.Count;
			}

			if ( TranslucentVertices.Count > 0 )
			{
				TranslucentMesh.SetVertexBufferData( TranslucentVertices, translucentVertexCount );
				translucentVertexCount += TranslucentVertices.Count;
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
					TranslucentSceneObject?.SetValue( name, neighbour.LightMap.Texture );
					OpaqueSceneObject?.SetValue( name, neighbour.LightMap.Texture );
					if ( recurseNeighbours ) neighbour.UpdateAdjacents();
				}
			}
		}

		private void BuildCollision()
		{
			if ( Body.IsValid() )
			{
				if ( Shape != null )
				{
					Body.RemoveShape( Shape, false );
					Shape = null;
				}

				if ( CollisionVertices.Count > 0 && CollisionIndices.Count > 0 )
				{
					Shape = Body.AddMeshShape( CollisionVertices.ToArray(), CollisionIndices.ToArray() );
				}
			}
		}

		static readonly int[] BackFaceBlockIndices = new[]
		{
			0, 1, 2, 2, 3, 0,
			7, 6, 5, 5, 4, 7,
			3, 7, 4, 4, 0, 3,
			1, 5, 6, 6, 2, 1,
			0, 4, 5, 5, 1, 0,
			2, 6, 7, 7, 3, 2,
		};

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

		public void UpdateFaceVertices()
		{
			OpaqueVertices.Clear();
			TranslucentVertices.Clear();
			CollisionVertices.Clear();
			CollisionIndices.Clear();

			var faceWidth = 1;
			var faceHeight = 1;

			for ( var x = 0; x < ChunkSize; x++ )
			{
				for ( var y = 0; y < ChunkSize; y++ )
				{
					for ( var z = 0; z < ChunkSize; z++ )
					{
						var position = new IntVector3( x, y, z );
						var blockId = GetLocalPositionBlock( position );
						if ( blockId == 0 ) continue;

						var block = Map.GetBlockType( blockId );

						for ( int faceSide = 0; faceSide < 6; faceSide++ )
						{
							var textureId = block.GetTextureId( (BlockFace)faceSide, this, x, y, z );
							var normal = (byte)faceSide;
							var faceData = (uint)((textureId & 31) << 18 | (0 & 15) << 23 | (normal & 7) << 27);
							var collisionIndex = CollisionIndices.Count;
							var neighbourId = Map.GetAdjacentBlock( Offset + position, faceSide );
							var neighbourBlock = Map.GetBlockType( neighbourId );
							var axis = BlockDirectionAxis[faceSide];
							var uAxis = (axis + 1) % 3;
							var vAxis = (axis + 2) % 3;

							if ( !neighbourBlock.IsTranslucent )
								continue;

							for ( int i = 0; i < 6; ++i )
							{
								var vi = BlockIndices[(faceSide * 6) + i];
								var vOffset = BlockVertices[vi];

								vOffset[uAxis] *= faceWidth;
								vOffset[vAxis] *= faceHeight;

								var vertex = new BlockVertex( (uint)(x + vOffset.x), (uint)(y + vOffset.y), (uint)(z + vOffset.z), (uint)x, (uint)y, (uint)z, faceData );

								if ( block.IsTranslucent )
									TranslucentVertices.Add( vertex );
								else
									OpaqueVertices.Add( vertex );

								if ( !block.IsPassable )
								{
									CollisionVertices.Add( new Vector3( (x + vOffset.x) + Offset.x, (y + vOffset.y) + Offset.y, (z + vOffset.z) + Offset.z ) * VoxelSize );
									CollisionIndices.Add( collisionIndex + i );
								}
							}

							/*
							if ( !block.CullBackFaces )
							{
								for ( int i = 0; i < 6; ++i )
								{
									int vi = BackFaceBlockIndices[(face * 6) + i];
									var vOffset = BlockVertices[vi];

									vOffset[widthAxis] *= width;
									vOffset[heightAxis] *= height;

									var vertex = new BlockVertex( (uint)(x + vOffset.x), (uint)(y + vOffset.y), (uint)(z + vOffset.z), (uint)x, (uint)y, (uint)z, faceData );

									if ( block.IsTranslucent )
										TranslucentVertices.Add( vertex );
									else
										OpaqueVertices.Add( vertex );
								}
							}
							*/
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

		[Event.Tick]
		private void Tick()
		{
			if ( QueueRebuild )
			{
				Build();
				QueueRebuild = false;
			}
		}
	}
}
