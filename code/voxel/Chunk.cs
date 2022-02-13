using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Chunk : IValid
	{
		public struct ChunkVertexData
		{
			public BlockVertex[] TranslucentVertices;
			public BlockVertex[] OpaqueVertices;
			public Vector3[] CollisionVertices;
			public int[] CollisionIndices;
			public bool IsValid;
		}

		public static readonly int ChunkSize = 32;
		public static readonly int VoxelSize = 48;

		public Dictionary<IntVector3, BlockData> Data { get; set; } = new();
		public HashSet<IntVector3> DirtyData { get; set; } = new();
		public bool HasDoneFirstFullUpdate { get; set; }
		public bool IsFullUpdateActive { get; set; }
		public ChunkVertexData UpdateVerticesResult { get; set; }
		public bool QueueRebuild { get; set; }
		public bool IsModelCreated { get; private set; }
		public bool Initialized { get; private set; }
		public Biome Biome { get; set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public ChunkLightMap LightMap { get; set; }
		public byte[] Blocks;
		public IntVector3 Offset;
		public int Index;
		public Map Map;

		public PhysicsBody Body;
		public PhysicsShape Shape;

		private int[] Heightmap;
		private FastNoiseLite Noise1;
		private FastNoiseLite Noise2;
		private FastNoiseLite Noise3;
		private FastNoiseLite Noise4;

		private Dictionary<int, BlockEntity> Entities { get; set; }
		private SceneObject TranslucentSceneObject { get; set; }
		private SceneObject OpaqueSceneObject { get; set; }
		private ModelBuilder TranslucentModelBuilder { get; set; }
		private Model TranslucentModel { get; set; }
		private ModelBuilder OpaqueModelBuilder { get; set; }
		private object VertexLock = new object();
		private bool QueuedFullUpdate { get; set; }
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

			SetupHeightmap();
		}

		public async Task Initialize()
		{
			if ( Initialized )
				return;

			if ( IsClient )
			{
				await GameTask.RunInThreadAsync( StartInitialLightingTask );

				LightMap.UpdateTorchLight();
				LightMap.UpdateSunLight();
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

			Event.Register( this );

			if ( IsClient )
			{
				QueueNeighbourFullUpdate();
			}
		}

		public void GenerateHeightmap()
		{
			for ( int y = 0; y < ChunkSize; y++ )
			{
				for ( int x = 0; x < ChunkSize; x++ )
				{
					var n1 = Noise1.GetNoise( x + Offset.x, y + Offset.y );
					var n2 = Noise2.GetNoise( x + Offset.x, y + Offset.y );
					var n3 = Noise3.GetNoise( x + Offset.x, y + Offset.y );
					var n4 = Noise4.GetNoise( x + Offset.x, y + Offset.y );
					Heightmap[x + y * ChunkSize] = (int)( (n1 + (n2 * n3 * (n4 * 2 - 1))) * 64 + 64 );
				}
			}
		}

		public void GeneratePerlin()
		{
			Rand.SetSeed( Offset.x + Offset.y + Offset.z * ChunkSize + Map.Seed );

			Log.Info( $"Generating Chunk {Offset}" );

			var topChunk = GetNeighbour( BlockFace.Top );

			for ( var x = 0; x < ChunkSize; x++ )
			{
				for ( var y = 0; y < ChunkSize; y++ )
				{
					var biome = Map.GetBiomeAt( x + Offset.x, y + Offset.y );
					var h = GetHeight( x, y );

					for ( var z = 0; z < ChunkSize; z++ )
					{
						var index = GetLocalPositionIndex( x, y, z );
						var position = new IntVector3( x, y, z );

						if ( z + Offset.z > h )
						{
							if ( z + Offset.z < Map.SeaLevel )
							{
								CreateBlockAtPosition( position, biome.LiquidBlockId );
							}
							else if ( Blocks[index] == 0 && z == ChunkSize - 1 )
							{
								//LightMap.AddSunLight( position, 15 );
							}
						}
						else
						{
							var isGeneratingTopBlock = z + Offset.z == h && z + Offset.z > Map.SeaLevel - 1;

							if ( isGeneratingTopBlock )
								CreateBlockAtPosition( position, biome.TopBlockId );
							else if ( z + Offset.z <= Map.SeaLevel - 1 && h < Map.SeaLevel && z + Offset.z > h - 3 )
								CreateBlockAtPosition( position, biome.BeachBlockId );
							else if ( z + Offset.z > h - 3 )
								CreateBlockAtPosition( position, biome.GroundBlockId );
							else
								CreateBlockAtPosition( position, biome.UndergroundBlockId );

							GenerateCaves( biome, x, y, z );

							if ( isGeneratingTopBlock && Blocks[index] > 0 )
							{
								if ( Rand.Float() < 0.01f )
								{
									GenerateTree( biome, position.x, position.y, position.z );
								}
							}
						}

						if ( topChunk.IsValid() && topChunk.Initialized )
						{
							var sunlightLevel = topChunk.LightMap.GetSunLight( new IntVector3( x, y, 0 ) );

							if ( sunlightLevel > 0 )
							{
								//LightMap.AddSunLight( new IntVector3( x, y, ChunkSize - 1 ), sunlightLevel );
							}
						}
					}
				}
			}
		}

		public bool GenerateCaves( Biome biome, int x, int y, int z )
		{
			if ( !Map.IsInside( x, y, z ) ) return false;

			var localPosition = new IntVector3( x, y, z );
			var position = Offset + new IntVector3( x, y, z );
			int rx = localPosition.x + Offset.x;
			int ry = localPosition.y + Offset.y;
			int rz = localPosition.z + Offset.z;

			double n1 = Map.CaveNoise.GetNoise( rx, ry, rz );
			double n2 = Map.CaveNoise.GetNoise( rx, ry + 88f, rz );
			double finalNoise = n1 * n1 + n2 * n2;

			if ( finalNoise < 0.02f )
			{
				CreateBlockAtPosition( position, 0 );
				return true;
			}

			return false;
		}

		public void GenerateTree( Biome biome, int x, int y, int z )
		{
			var minTrunkHeight = 3;
			var maxTrunkHeight = 6;
			var minLeavesRadius = 1;
			var maxLeavesRadius = 2;
			int trunkHeight = Rand.Int( minTrunkHeight, maxTrunkHeight );
			int trunkTop = z + trunkHeight;
			int leavesRadius = Rand.Int( minLeavesRadius, maxLeavesRadius );

			// Would we be trying to generate a tree in another chunk?
			if ( z + trunkHeight + leavesRadius >= ChunkSize
				|| x < leavesRadius || x > ChunkSize - leavesRadius
				|| y < leavesRadius || y > ChunkSize - leavesRadius )
			{
				return;
			}

			for ( int trunkZ = z + 1; trunkZ < trunkTop; trunkZ++ )
			{
				if ( IsInside( x, y, trunkZ ) )
				{
					CreateBlockAtPosition( new IntVector3( x, y, trunkZ ), biome.TreeLogBlockId );
				}
			}

			for ( int leavesX = x - leavesRadius; leavesX <= x + leavesRadius; leavesX++ )
			{
				for ( int leavesY = y - leavesRadius; leavesY <= y + leavesRadius; leavesY++ )
				{
					for ( int leavesZ = trunkTop; leavesZ <= trunkTop + leavesRadius; leavesZ++ )
					{
						if ( IsInside( leavesX, leavesY, leavesZ ) )
						{
							if (
								IsEmpty( leavesX, leavesY, leavesZ ) &&
								(leavesX != x - leavesRadius || leavesY != y - leavesRadius) &&
								(leavesX != x + leavesRadius || leavesY != y + leavesRadius) &&
								(leavesX != x + leavesRadius || leavesY != y - leavesRadius) &&
								(leavesX != x - leavesRadius || leavesY != y + leavesRadius)
							)
							{
								var position = new IntVector3( leavesX, leavesY, leavesZ );
								CreateBlockAtPosition( position, biome.TreeLeafBlockId );
							}
						}
					}
				}
			}

			for ( int leavesX = x - (leavesRadius - 1); leavesX <= x + (leavesRadius - 1); leavesX++ )
			{
				for ( int leavesY = y - (leavesRadius - 1); leavesY <= y + (leavesRadius - 1); leavesY++ )
				{
					var position = new IntVector3( leavesX, leavesY, trunkTop + leavesRadius + 1 );

					if ( Map.IsInside( position ) && Map.IsEmpty( position ) )
					{
						CreateBlockAtPosition( position, biome.TreeLeafBlockId );
					}
				}
			}
		}

		public int GetHeight( int x, int y )
		{
			return Heightmap[x + y * ChunkSize];
		}

		public void SetHeight( int x, int y, int height )
		{
			Heightmap[x + y * ChunkSize] = height;
		}

		public bool IsEmpty( int lx, int ly, int lz )
		{
			if ( !IsInside( ly, ly, lz ) ) return true;
			var index = GetLocalPositionIndex( lx, ly, lz );
			return Blocks[index] == 0;
		}

		public bool IsInside( int lx, int ly, int lz )
		{
			if ( lx < 0 || ly < 0 || lz < 0 )
				return false;

			if ( lx >= ChunkSize || ly >= ChunkSize || lz >= ChunkSize )
				return false;

			return true;
		}

		public bool IsInside( IntVector3 localPosition )
		{
			if ( localPosition.x < 0 || localPosition.y < 0 || localPosition.z < 0 )
				return false;

			if ( localPosition.x >= ChunkSize || localPosition.y >= ChunkSize || localPosition.z >= ChunkSize )
				return false;

			return true;
		}

		public bool IsFullUpdateTaskRunning()
		{
			return IsFullUpdateActive;
		}

		public void QueueFullUpdate()
		{
			if ( !HasDoneFirstFullUpdate ) return;
			QueuedFullUpdate = true;
		}

		public async void FullUpdate()
		{
			if ( IsFullUpdateTaskRunning() ) return;

			IsFullUpdateActive = true;
			QueuedFullUpdate = false;

			await GameTask.RunInThreadAsync( StartFullUpdateTask );

			IsFullUpdateActive = false;
			QueueRebuild = true;
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

		public IEnumerable<Chunk> GetNeighbours()
		{
			var currentOffset = Offset;
			currentOffset.x--;

			if ( Map.IsInside( currentOffset ) )
			{
				var index = Map.GetChunkIndex( currentOffset );
				yield return Map.Chunks[index];
			}

			currentOffset.x++;
			currentOffset.y--;

			if ( Map.IsInside( currentOffset ) )
			{
				var index = Map.GetChunkIndex( currentOffset );
				yield return Map.Chunks[index];
			}

			currentOffset.y++;
			currentOffset.y += ChunkSize + 1;

			if ( Map.IsInside( currentOffset ) )
			{
				var index = Map.GetChunkIndex( currentOffset );
				yield return Map.Chunks[index];
			}

			currentOffset.y -= ChunkSize + 1;
			currentOffset.x += ChunkSize + 1;

			if ( Map.IsInside( currentOffset ) )
			{
				var index = Map.GetChunkIndex( currentOffset );
				yield return Map.Chunks[index];
			}

			currentOffset.x -= ChunkSize + 1;
			currentOffset.z += ChunkSize + 1;

			if ( Map.IsInside( currentOffset ) )
			{
				var index = Map.GetChunkIndex( currentOffset );
				yield return Map.Chunks[index];
			}

			currentOffset.z -= ChunkSize + 1;
			currentOffset.z--;

			if ( Map.IsInside( currentOffset ) )
			{
				var index = Map.GetChunkIndex( currentOffset );
				yield return Map.Chunks[index];
			}
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
				neighbour.QueueFullUpdate();
			}
		}

		public void Build()
		{
			BuildMesh();

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

			QueueRebuild = false;
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

			if ( Body.IsValid() && Shape.IsValid() )
			{
				Body.RemoveShape( Shape );
				Shape = null;
			}

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

		public void BuildCollision()
		{
			lock ( VertexLock )
			{
				if ( !UpdateVerticesResult.IsValid ) return;
				if ( !Body.IsValid() ) return;

				var collisionVertices = UpdateVerticesResult.CollisionVertices;
				var collisionIndices = UpdateVerticesResult.CollisionIndices;
				var oldShape = Shape;

				if ( collisionVertices.Length > 0 && collisionIndices.Length > 0 )
				{
					Shape = Body.AddMeshShape( collisionVertices, collisionIndices );
				}

				if ( oldShape.IsValid() )
				{
					Body.RemoveShape( oldShape );
				}
			}
		}

		public void BuildMesh()
		{
			Host.AssertClient();

			lock ( VertexLock )
			{
				if ( !UpdateVerticesResult.IsValid ) return;

				if ( !OpaqueMesh.IsValid || !TranslucentMesh.IsValid )
					return;

				var translucentVertices = UpdateVerticesResult.TranslucentVertices;
				var opaqueVertices = UpdateVerticesResult.OpaqueVertices;

				int translucentVertexCount = translucentVertices.Length;
				int opaqueVertexCount = opaqueVertices.Length;

				try
				{
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

					if ( opaqueVertices.Length > 0 )
					{
						OpaqueMesh.SetVertexBufferData( new Span<BlockVertex>( opaqueVertices ), opaqueVertexCount );
						opaqueVertexCount += opaqueVertices.Length;
					}

					if ( translucentVertices.Length > 0 )
					{
						TranslucentMesh.SetVertexBufferData( new Span<BlockVertex>( translucentVertices ), translucentVertexCount );
						translucentVertexCount += translucentVertices.Length;
					}

					OpaqueMesh.SetVertexRange( 0, opaqueVertexCount );
					TranslucentMesh.SetVertexRange( 0, translucentVertexCount );
				}
				catch ( Exception e )
				{
					Log.Error( e );
				}
			}
		}

		private void SetupHeightmap()
		{
			Heightmap = new int[ChunkSize * ChunkSize];

			Noise1 = new FastNoiseLite( Map.Seed );
			Noise1.SetNoiseType( FastNoiseLite.NoiseType.OpenSimplex2 );
			Noise1.SetFractalType( FastNoiseLite.FractalType.FBm );
			Noise1.SetFractalOctaves( 4 );
			Noise1.SetFrequency( 1 / 256.0f );

			Noise2 = new FastNoiseLite( Map.Seed );
			Noise2.SetNoiseType( FastNoiseLite.NoiseType.OpenSimplex2 );
			Noise2.SetFractalType( FastNoiseLite.FractalType.FBm );
			Noise2.SetFractalOctaves( 4 );
			Noise2.SetFrequency( 1 / 256.0f );

			Noise3 = new FastNoiseLite( Map.Seed );
			Noise3.SetNoiseType( FastNoiseLite.NoiseType.OpenSimplex2 );
			Noise3.SetFractalType( FastNoiseLite.FractalType.FBm );
			Noise3.SetFractalOctaves( 4 );
			Noise3.SetFrequency( 1 / 256.0f );

			Noise4 = new FastNoiseLite( Map.Seed );
			Noise4.SetNoiseType( FastNoiseLite.NoiseType.OpenSimplex2 );
			Noise4.SetFrequency( 1 / 1024.0f );
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

		public Task<ChunkVertexData> StartUpdateVerticesTask()
		{
			lock ( VertexLock )
			{
				var translucentVertices = new List<BlockVertex>();
				var opaqueVertices = new List<BlockVertex>();
				var collisionVertices = new List<Vector3>();
				var collisionIndices = new List<int>();

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
								var neighbourId = Map.GetAdjacentBlock( Offset + position, faceSide );
								var neighbourBlock = Map.GetBlockType( neighbourId );

								if ( !neighbourBlock.IsTranslucent )
									continue;

								var collisionIndex = collisionIndices.Count;
								var textureId = block.GetTextureId( (BlockFace)faceSide, this, x, y, z );
								var normal = (byte)faceSide;
								var faceData = (uint)((textureId & 31) << 18 | (0 & 15) << 23 | (normal & 7) << 27);
								var axis = BlockDirectionAxis[faceSide];
								var uAxis = (axis + 1) % 3;
								var vAxis = (axis + 2) % 3;

								for ( int i = 0; i < 6; ++i )
								{
									var vi = BlockIndices[(faceSide * 6) + i];
									var vOffset = BlockVertices[vi];

									vOffset[uAxis] *= faceWidth;
									vOffset[vAxis] *= faceHeight;

									if ( IsClient && block.HasTexture )
									{
										var vertex = new BlockVertex( (uint)(x + vOffset.x), (uint)(y + vOffset.y), (uint)(z + vOffset.z), (uint)x, (uint)y, (uint)z, faceData );

										if ( block.IsTranslucent )
											translucentVertices.Add( vertex );
										else
											opaqueVertices.Add( vertex );
									}

									if ( !block.IsPassable )
									{
										collisionVertices.Add( new Vector3( (x + vOffset.x) + Offset.x, (y + vOffset.y) + Offset.y, (z + vOffset.z) + Offset.z ) * VoxelSize );
										collisionIndices.Add( collisionIndex + i );
									}
								}
							}
						}
					}
				}

				return GameTask.FromResult( new ChunkVertexData
				{
					TranslucentVertices = translucentVertices.ToArray(),
					OpaqueVertices = opaqueVertices.ToArray(),
					CollisionVertices = collisionVertices.ToArray(),
					CollisionIndices = collisionIndices.ToArray(),
					IsValid = true
				} );
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
						writer.Write( DirtyData.Count );

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
			LightMap.UpdateTorchLight();
			LightMap.UpdateSunLight();

			if ( IsFullUpdateTaskRunning() ) return;

			if ( QueueRebuild && !AreAdjacentChunksUpdating() )
			{
				Build();
			}

			if ( !QueueRebuild && HasDoneFirstFullUpdate )
			{
				LightMap.UpdateTexture();
			}
		}

		[Event.Tick]
		private void Tick()
		{
			if ( QueuedFullUpdate )
			{
				FullUpdate();
			}
		}

		private void CreateBlockAtPosition( IntVector3 localPosition, byte blockId )
		{
			if ( !IsInside( localPosition ) ) return;

			var position = Offset + localPosition;
			var block = Map.GetBlockType( blockId );

			SetBlock( localPosition, blockId );
			block.OnBlockAdded( this, position.x, position.y, position.z, (int)BlockFace.Top );

			var entityName = IsServer ? block.ServerEntity : block.ClientEntity;

			if ( !string.IsNullOrEmpty( entityName ) )
			{
				var entity = Library.Create<BlockEntity>( entityName );
				entity.BlockType = block;
				SetEntity( localPosition, entity );
			}
		}

		private async Task StartInitialLightingTask()
		{
			PropagateSunlight();
			PerformFullTorchUpdate();

			await GameTask.Delay( 1 );
		}

		private bool AreAdjacentChunksUpdating()
		{
			return GetNeighbours().Any( c => c.IsFullUpdateTaskRunning() );
		}

		private async Task StartFullUpdateTask()
		{
			try
			{
				UpdateVerticesResult = await StartUpdateVerticesTask();
				BuildCollision();

				await GameTask.Delay( 1 );
			}
			catch ( TaskCanceledException e )
			{
				return;
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
		}
	}
}
