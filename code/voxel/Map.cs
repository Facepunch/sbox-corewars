using Facepunch.CoreWars.Blocks;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Map : IValid
	{
		public delegate void OnInitializedCallback();
		public event OnInitializedCallback OnInitialized;

		public static Map Current { get; private set; }

		public static Map Create()
		{
			return new Map();
		}

		[ClientRpc]
		public static void Receive( byte[] data )
		{
			if ( Current != null )
			{
				Current.Destroy();
			}

			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					Current = new Map
					{
						SizeX = reader.ReadInt32(),
						SizeY = reader.ReadInt32(),
						SizeZ = reader.ReadInt32(),
						GreedyMeshing = reader.ReadBoolean()
					};

					Current.LoadBlockAtlas( reader.ReadString() );

					var types = reader.ReadInt32();

					for ( var i = 0; i < types; i++ )
					{
						var id = reader.ReadByte();
						var name = reader.ReadString();
						var type = Library.Create<BlockType>( name );

						type.Initialize();

						Log.Info( $"[Client] Initializing block type {name} with id #{id}" );

						Current.BlockTypes.Add( name, id );
						Current.BlockData.Add( id, type );
					}
				}
			}

			Current.Init();
		}

		[ClientRpc]
		public static void ReceiveDataUpdate( int chunkIndex, byte[] data )
		{
			if ( Current == null ) return;

			Current.Chunks[chunkIndex].DeserializeData( data );
			Log.Info( "Received Data Update" );
		}

		[ClientRpc]
		public static void SetBlockOnClient( int x, int y, int z, byte blockId, int direction )
		{
			Host.AssertClient();
			Current?.SetBlockAndUpdate( new IntVector3( x, y, z ), blockId, direction, true );
		}

		public static BBox ToSourceBBox( IntVector3 position )
		{
			var sourcePosition = ToSourcePosition( position );
			var sourceMins = sourcePosition;
			var sourceMaxs = sourcePosition + Vector3.One * Chunk.VoxelSize;

			return new BBox( sourceMins, sourceMaxs );
		}

		public static Vector3 ToSourcePosition( IntVector3 position )
		{
			return new Vector3( position.x * Chunk.VoxelSize, position.y * Chunk.VoxelSize, position.z * Chunk.VoxelSize );
		}

		public static IntVector3 ToVoxelPosition( Vector3 position )
		{
			var fPosition = position * (1.0f / Chunk.VoxelSize);
			return new IntVector3( (int)fPosition.x, (int)fPosition.y, (int)fPosition.z );
		}

		public List<Vector3> SuitableSpawnPositions { get; private set; } = new();
		public Dictionary<byte, BlockType> BlockData { get; private set; } = new();
		public Dictionary<string, byte> BlockTypes { get; private set; } = new();
		public BlockAtlas BlockAtlas { get; private set; }
		public bool GreedyMeshing { get; private set; }
		public bool Initialized { get; private set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public int SizeX;
		public int SizeY;
		public int SizeZ;
		public int NumChunksX;
		public int NumChunksY;
		public int NumChunksZ;
		public Chunk[] Chunks;

		private string BlockAtlasFileName { get; set; }
		private byte NextAvailableBlockId { get; set; }
		private FastNoiseLite CaveNoise { get; set; }

		public bool IsValid => true;

		private Map()
		{
			BlockTypes[typeof( AirBlock ).Name] = NextAvailableBlockId;
			BlockData[NextAvailableBlockId] = new AirBlock( this );

			CaveNoise = new();
			CaveNoise.SetNoiseType( FastNoiseLite.NoiseType.OpenSimplex2 );
			CaveNoise.SetFractalType( FastNoiseLite.FractalType.FBm );
			CaveNoise.SetFractalOctaves( 2 );
			CaveNoise.SetFrequency( 1f / 128f );

			NextAvailableBlockId++;
			Current = this;
		}

		public void Send( Client client )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( SizeX );
					writer.Write( SizeY );
					writer.Write( SizeZ );
					writer.Write( GreedyMeshing );
					writer.Write( BlockAtlasFileName );
					writer.Write( BlockData.Count - 1 );

					foreach ( var kv in BlockData )
					{
						if ( kv.Key == 0 ) continue;

						writer.Write( kv.Key );
						writer.Write( kv.Value.GetType().Name );
					}
				}

				Receive( To.Single( client ), stream.GetBuffer() );
			}
		}

		public bool SetBlockInDirection( Vector3 origin, Vector3 direction, byte blockId, bool checkSourceCollision = false )
		{
			var face = Trace( origin * (1.0f / Chunk.VoxelSize), direction.Normal, 10000f, out var endPosition, out _ );

			if ( face == BlockFace.Invalid )
				return false;

			var position = blockId != 0 ? GetAdjacentPosition( endPosition, (int)face ) : endPosition;

			if ( checkSourceCollision )
			{
				var bbox = ToSourceBBox( position );

				if ( Physics.GetEntitiesInBox( bbox ).Any() )
					return false;
			}

			SetBlockOnServer( position.x, position.y, position.z, blockId, (int)face );
			return true;
		}

		public bool GetBlockInDirection( Vector3 origin, Vector3 direction, out IntVector3 position )
		{
			var face = Trace( origin * (1.0f / Chunk.VoxelSize), direction.Normal, 10000f, out position, out _ );
			return (face != BlockFace.Invalid);
		}

		public void SetBlockOnServer( int x, int y, int z, byte blockId, int direction )
		{
			Host.AssertServer();

			var position = new IntVector3( x, y, z );

			if ( SetBlockAndUpdate( position, blockId, direction ) )
			{
				SetBlockOnClient( x, y, z, blockId, direction );
			}
		}

		public byte FindBlockId<T>() where T : BlockType
		{
			if ( BlockTypes.TryGetValue( typeof( T ).Name, out var id ) )
				return id;
			else
				return 0;
		}

		public void LoadBlockAtlas( string fileName )
		{
			if ( BlockAtlas != null )
				throw new Exception( "Unable to load a block atlas as one is already loaded for this map!" );

			BlockAtlasFileName = fileName;
			BlockAtlas = FileSystem.Mounted.ReadJsonOrDefault<BlockAtlas>( fileName );
			BlockAtlas.Initialize();
		}

		public void AddBlockType( BlockType type )
		{
			Host.AssertServer();

			if ( BlockAtlas == null )
				throw new Exception( "Unable to add any block types with no loaded block atlas!" );

			Log.Info( $"[Server] Initializing block type {type.GetType().Name} with id #{NextAvailableBlockId}" );

			type.Initialize();

			BlockTypes[type.GetType().Name] = NextAvailableBlockId;
			BlockData[NextAvailableBlockId] = type;
			NextAvailableBlockId++;
		}

		public async void ReceiveChunk( int index, byte[] blocks, byte[] data )
		{
			var chunk = Chunks[index];

			chunk.Blocks = blocks;
			chunk.DeserializeData( data );

			await chunk.Init();
		}

		public void AddAllBlockTypes()
		{
			Host.AssertServer();

			if ( BlockAtlas == null )
				throw new Exception( "Unable to add any block types with no loaded block atlas!" );

			foreach ( var type in Library.GetAll<BlockType>() )
			{
				AddBlockType( Library.Create<BlockType>( type ) );
			}
		}

		public void SetSize( int sizeX, int sizeY, int sizeZ )
		{
			SizeX = sizeX;
			SizeY = sizeY;
			SizeZ = sizeZ;

			NumChunksX = SizeX / Chunk.ChunkSize;
			NumChunksY = SizeY / Chunk.ChunkSize;
			NumChunksZ = SizeZ / Chunk.ChunkSize;

			SetupChunks();
		}

		public Voxel GetVoxel( IntVector3 position )
		{
			return GetVoxel( position.x, position.y, position.z );
		}

		public Voxel GetVoxel( int x, int y, int z )
		{
			if ( !IsInside( x, y, z ) ) return new Voxel();
			var chunkIndex = GetChunkIndex( x, y, z );
			return Chunks[chunkIndex].GetVoxel( x % Chunk.ChunkSize, y % Chunk.ChunkSize, z % Chunk.ChunkSize );
		}

		public T GetOrCreateData<T>( IntVector3 position ) where T : BlockData
		{
			if ( !IsInside( position ) ) return null;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].GetOrCreateData<T>( localPosition );
		}

		public T GetData<T>( IntVector3 position ) where T : BlockData
		{
			if ( !IsInside( position ) ) return null;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].GetData<T>( localPosition );
		}

		public byte GetSunLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.GetSunLight( localPosition );
		}

		public bool SetSunLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return false;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.SetSunLight( localPosition, value );
		}

		public byte GetTorchLight( IntVector3 position, int channel )
		{
			if ( !IsInside( position ) ) return 0;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.GetTorchLight( localPosition, channel );
		}

		public bool SetTorchLight( IntVector3 position, int channel, byte value )
		{
			if ( !IsInside( position ) ) return false;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.SetTorchLight( localPosition, channel, value );
		}

		public byte GetRedTorchLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.GetRedTorchLight( localPosition );
		}

		public bool SetRedTorchLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return false;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.SetRedTorchLight( localPosition, value );
		}

		public byte GetGreenTorchLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.GetGreenTorchLight( localPosition );
		}

		public bool SetGreenTorchLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return false;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.SetGreenTorchLight( localPosition, value );
		}

		public byte GetBlueTorchLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.GetGreenTorchLight( localPosition );
		}

		public bool SetBlueTorchLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return false;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			return Chunks[chunkIndex].LightMap.SetGreenTorchLight( localPosition, value );
		}

		public void RemoveSunLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.RemoveSunLight( localPosition );
		}

		public void RemoveTorchLight( IntVector3 position, int channel )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.RemoveTorchLight( localPosition, channel );
		}

		public void RemoveRedTorchLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.RemoveRedTorchLight( localPosition );
		}

		public void RemoveGreenTorchLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.RemoveGreenTorchLight( localPosition );
		}

		public void RemoveBlueTorchLight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.RemoveBlueTorchLight( localPosition );
		}

		public void AddSunLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.AddSunLight( localPosition, value );
		}

		public void AddTorchLight( IntVector3 position, int channel, byte value )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.AddTorchLight( localPosition, channel, value );
		}

		public void AddRedTorchLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.AddRedTorchLight( localPosition, value );
		}

		public void AddGreenTorchLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.AddGreenTorchLight( localPosition, value );
		}

		public void AddBlueTorchLight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return;
			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			Chunks[chunkIndex].LightMap.AddBlueTorchLight( localPosition, value );
		}

		public void Destroy()
		{
			foreach ( var chunk in Chunks )
			{
				chunk.Destroy();
			}

			Event.Unregister( this );
		}

		public void Init()
		{
			if ( Initialized ) return;

			NumChunksX = SizeX / Chunk.ChunkSize;
			NumChunksY = SizeY / Chunk.ChunkSize;
			NumChunksZ = SizeZ / Chunk.ChunkSize;

			if ( Chunks == null )
			{
				SetupChunks();
			}

			if ( IsServer )
			{
				foreach ( var chunk in Chunks )
				{
					_ = chunk.Init();
				}
			}

			Initialized = true;
			OnInitialized?.Invoke();

			Event.Register( this );

			GameTask.RunInThreadAsync( ChunkFullUpdateTask );
		}

		public bool SetBlockAndUpdate( IntVector3 position, byte blockId, int direction, bool forceUpdate = false )
		{
			var chunkIndex = GetChunkIndex( position );
			var shouldBuild = false;
			var chunkIds = new HashSet<int>();

			if ( SetBlock( position, blockId, direction ) || forceUpdate )
			{
				shouldBuild = true;
				chunkIds.Add( chunkIndex );

				for ( int i = 0; i < 6; i++ )
				{
					var posInChunk = ToLocalPosition( position );
					Chunks[chunkIndex].UpdateBlockSlice( posInChunk, i );

					var adjacentPos = GetAdjacentPosition( position, i );
					var adjadentChunkIndex = GetChunkIndex( adjacentPos );
					var adjacentPosInChunk = ToLocalPosition( adjacentPos );

					chunkIds.Add( adjadentChunkIndex );
					Chunks[adjadentChunkIndex].UpdateBlockSlice( adjacentPosInChunk, GetOppositeDirection( i ) );
				}
			}

			foreach ( var chunkid in chunkIds )
			{
				Chunks[chunkid].Build();
			}

			return shouldBuild;
		}

		public int GetChunkIndex( int x, int y, int z )
		{
			return (x / Chunk.ChunkSize) + (y / Chunk.ChunkSize) * NumChunksX + (z / Chunk.ChunkSize) * NumChunksX * NumChunksY;
		}

		public int GetChunkIndex( IntVector3 position )
		{
			return GetChunkIndex( position.x, position.y, position.z );
		}

		public static IntVector3 ToLocalPosition( IntVector3 position )
		{
			return new IntVector3( position.x % Chunk.ChunkSize, position.y % Chunk.ChunkSize, position.z % Chunk.ChunkSize );
		}

		public static BlockFace GetOppositeDirection( BlockFace direction )
		{
			return (BlockFace)GetOppositeDirection( (int)direction );
		}

		public static int GetOppositeDirection( int direction )
		{
			return direction + ((direction % 2 != 0) ? -1 : 1);
		}

		public struct PerlinGenerationConfig
		{
			public byte GroundBlockId;
			public byte[] UndergroundBlockIds;
			public byte BedrockId;
		}

		public async Task GeneratePerlin( PerlinGenerationConfig config )
		{
			byte undergroundBlock;

			for ( int x = 0; x < SizeX; x++ )
			{
				await GameTask.Delay( 1 );

				for ( int y = 0; y < SizeY; y++ )
				{
					int height = (int)((SizeZ * 0.5f) * (Noise.Perlin( (x * 64) * 0.001f, (y * 64) * 0.001f, 0 ) + 1f) * 0.5f);
					if ( height <= 0 ) height = 0;
					if ( height > SizeZ ) height = SizeZ;

					for ( int z = 0; z < SizeZ; z++ )
					{
						var position = new IntVector3( x, y, z );

						if ( IsEmpty( position ) )
						{
							SetBlockAtPosition( position, (byte)(z < height ? config.GroundBlockId : 0) );

							if ( z < height / 2 )
							{
								undergroundBlock = config.UndergroundBlockIds[Rand.Int( config.UndergroundBlockIds.Length - 1 )];
								SetBlockAtPosition( position, undergroundBlock );
								GenerateCaves( x, y, z );
							}
						}
					}

					if ( Rand.Float( 100f ) <= 1f )
						GenerateTree( x, y, height - 1 );

					var topPosition = new IntVector3( x, y, height );

					if ( IsInside( topPosition ) && IsEmpty( topPosition ) )
					{
						SuitableSpawnPositions.Add( ToSourcePosition( new IntVector3( x, y, height + 1 ) ) );
					}

					SetBlockAtPosition( new IntVector3( x, y, 0 ), config.BedrockId );
				}
			}
		}

		public bool GenerateCaves( int x, int y, int z )
		{
			if ( !IsInside( x, y, z ) ) return false;

			var position = new IntVector3( x, y, z );
			var chunkIndex = GetChunkIndex( position );
			var chunk = Chunks[chunkIndex];
			var localPosition = ToLocalPosition( position );
			int rx = localPosition.x + chunk.Offset.x * Chunk.ChunkSize;
			int ry = localPosition.y + chunk.Offset.y * Chunk.ChunkSize;
			int rz = localPosition.z + chunk.Offset.z * Chunk.ChunkSize;

			double n1 = CaveNoise.GetNoise( rx, ry, rz );
			double n2 = CaveNoise.GetNoise( rx, ry + 88f, rz );
			double finalNoise = n1 * n1 + n2 * n2;

			if ( finalNoise < 0.02f )
			{
				SetBlockAtPosition( position, 0 );
				return true;
			}

			return false;
		}

		public void GenerateTree( int x, int y, int z )
		{
			var minTrunkHeight = 3;
			var maxTrunkHeight = 6;
			var minLeavesRadius = 1;
			var maxLeavesRadius = 2;
			int trunkHeight = Rand.Int( minTrunkHeight, maxTrunkHeight );
			int trunkTop = z + trunkHeight;
			int leavesRadius = Rand.Int( minLeavesRadius, maxLeavesRadius );

			for ( int trunkZ = z + 1; trunkZ < trunkTop; trunkZ++ )
			{
				if ( IsInside( x, y, trunkZ ) )
				{
					SetBlockAtPosition( new IntVector3( x, y, trunkZ ), FindBlockId<WoodBlock>() );
				}
			}

			for ( int leavesX = x - leavesRadius; leavesX <= x + leavesRadius; leavesX++ )
			{
				for ( int leavesY = y - leavesRadius; leavesY <= y + leavesRadius; leavesY++ )
				{
					for ( int leavesZ = trunkTop; leavesZ <= trunkTop + leavesRadius; leavesZ++ )
					{
						if ( IsInside( leavesX, leavesY, leavesZ  ) )
						{
							var position = new IntVector3( leavesX, leavesY, leavesZ );

							if (
								IsEmpty( position ) &&
								(leavesX != x - leavesRadius || leavesY != y - leavesRadius) &&
								(leavesX != x + leavesRadius || leavesY != y + leavesRadius) &&
								(leavesX != x + leavesRadius || leavesY != y - leavesRadius) &&
								(leavesX != x - leavesRadius || leavesY != y + leavesRadius)
							)
							{
								SetBlockAtPosition( position, FindBlockId<LeafBlock>() );
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

					if ( IsInside( position ) && IsEmpty( position ) )
					{
						SetBlockAtPosition( position, FindBlockId<LeafBlock>() );
					}
				}
			}
		}

		public byte GetBlock( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;

			var chunkIndex = GetChunkIndex( position );
			var chunk = Chunks[chunkIndex];

			return chunk.GetMapPositionBlock( position );
		}

		public bool IsInside( int x, int y, int z )
		{
			if ( x < 0 || x >= SizeX ) return false;
			if ( y < 0 || y >= SizeY ) return false;
			if ( z < 0 || z >= SizeZ ) return false;

			return true;
		}


		public bool IsInside( IntVector3 position )
		{
			return IsInside( position.x, position.y, position.z );
		}

		public bool SetBlock( IntVector3 position, byte blockId, int direction )
		{
			if ( !IsInside( position ) ) return false;

			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			var chunk = Chunks[chunkIndex];
			var blockIndex = chunk.GetLocalPositionIndex( localPosition );
			var currentBlockId = chunk.GetLocalIndexBlock( blockIndex );

			if ( blockId == currentBlockId ) return false;

			if ( (blockId != 0 && currentBlockId == 0) || (blockId == 0 && currentBlockId != 0) )
			{
				var block = GetBlockType( blockId );

				if ( IsClient )
				{
					RemoveRedTorchLight( position );
					RemoveGreenTorchLight( position );
					RemoveBlueTorchLight( position );
					RemoveSunLight( position );

					if ( block.LightLevel.x > 0 || block.LightLevel.y > 0 || block.LightLevel.z > 0 )
					{
						AddRedTorchLight( position, (byte)block.LightLevel.x );
						AddGreenTorchLight( position, (byte)block.LightLevel.y );
						AddBlueTorchLight( position, (byte)block.LightLevel.z );
					}
				}

				var currentBlock = GetBlockType( currentBlockId );
				currentBlock.OnBlockRemoved( chunk, position.x, position.y, position.z );

				chunk.Data.Remove( localPosition );
				chunk.SetBlock( blockIndex, blockId );
				chunk.LightMap.Update();

				block.OnBlockAdded( chunk, position.x, position.y, position.z, direction );

				var entityName = IsServer ? block.ServerEntity : block.ClientEntity;

				if ( !string.IsNullOrEmpty( entityName ) )
				{
					var entity = Library.Create<BlockEntity>( entityName );
					entity.BlockType = block;
					chunk.SetEntity( localPosition, entity );
				}
				else
				{
					chunk.RemoveEntity( localPosition );
				}

				return true;

			}

			return false;
		}

		public static IntVector3 GetAdjacentPosition( IntVector3 position, int side )
		{
			return position + Chunk.BlockDirections[side];
		}

		public BlockType GetBlockType( byte blockId )
		{
			if ( BlockData.TryGetValue( blockId, out var type ) )
				return type;
			else
				return null;
		}

		public byte GetAdjacentBlock( IntVector3 position, int side )
		{
			return GetBlock( GetAdjacentPosition( position, side ) );
		}

		public bool IsAdjacentEmpty( IntVector3 position, int side )
		{
			return IsEmpty( GetAdjacentPosition( position, side ) );
		}

		public bool IsEmpty( IntVector3 position )
		{
			if ( !IsInside( position ) ) return true;

			var chunkIndex = GetChunkIndex( position );
			var chunk = Chunks[chunkIndex];

			return chunk.GetMapPositionBlock( position ) == 0;
		}

		public BlockFace Trace( Vector3 position, Vector3 direction, float length, out IntVector3 hitPosition, out float distance )
		{
			hitPosition = new IntVector3( 0, 0, 0 );
			distance = 0;

			if ( direction.Length <= 0.0f )
			{
				return BlockFace.Invalid;
			}

			IntVector3 edgeOffset = new( direction.x < 0 ? 0 : 1,
				direction.y < 0 ? 0 : 1,
				direction.z < 0 ? 0 : 1 );

			IntVector3 stepAmount = new( direction.x < 0 ? -1 : 1,
				direction.y < 0 ? -1 : 1,
				direction.z < 0 ? -1 : 1 );

			IntVector3 faceDirection = new( direction.x < 0 ? (int)BlockFace.North : (int)BlockFace.South,
				direction.y < 0 ? (int)BlockFace.East : (int)BlockFace.West,
				direction.z < 0 ? (int)BlockFace.Top : (int)BlockFace.Bottom );

			Vector3 position3f = position;
			distance = 0;
			Ray ray = new( position, direction );

			while ( true )
			{
				IntVector3 position3i = new( (int)position3f.x, (int)position3f.y, (int)position3f.z );

				Vector3 distanceToNearestEdge = new( position3i.x - position3f.x + edgeOffset.x,
					position3i.y - position3f.y + edgeOffset.y,
					position3i.z - position3f.z + edgeOffset.z );

				for ( int i = 0; i < 3; ++i )
				{
					if ( MathF.Abs( distanceToNearestEdge[i] ) == 0.0f )
					{
						distanceToNearestEdge[i] = stepAmount[i];
					}
				}

				Vector3 lengthToNearestEdge = new( MathF.Abs( distanceToNearestEdge.x / direction.x ),
					MathF.Abs( distanceToNearestEdge.y / direction.y ),
					MathF.Abs( distanceToNearestEdge.z / direction.z ) );

				int axis;

				if ( lengthToNearestEdge.x < lengthToNearestEdge.y && lengthToNearestEdge.x < lengthToNearestEdge.z )
				{
					axis = 0;
				}
				else if ( lengthToNearestEdge.y < lengthToNearestEdge.x && lengthToNearestEdge.y < lengthToNearestEdge.z )
				{
					axis = 1;
				}
				else
				{
					axis = 2;
				}

				distance += lengthToNearestEdge[axis];
				position3f = position + direction * distance;
				position3f[axis] = MathF.Floor( position3f[axis] + 0.5f * stepAmount[axis] );

				if ( position3f.x < 0.0f || position3f.y < 0.0f || position3f.z < 0.0f ||
					 position3f.x >= SizeX || position3f.y >= SizeY || position3f.z >= SizeZ )
				{
					break;
				}

				BlockFace lastFace = (BlockFace)faceDirection[axis];

				if ( distance > length )
				{
					distance = length;

					return BlockFace.Invalid;
				}

				position3i = new( (int)position3f.x, (int)position3f.y, (int)position3f.z );

				byte blockId = GetBlock( position3i );

				if ( blockId != 0 )
				{
					hitPosition = position3i;
					return lastFace;
				}
			}

			Plane plane = new( new Vector3( 0.0f, 0.0f, 0.0f ), new Vector3( 0.0f, 0.0f, 1.0f ) );
			float distanceHit = 0;
			var traceHitPos = plane.Trace( ray, true );

			if ( traceHitPos.HasValue ) distanceHit = Vector3.DistanceBetween( position, traceHitPos.Value );

			if ( distanceHit >= 0.0f && distanceHit <= length )
			{
				Vector3 hitPosition3f = position + direction * distanceHit;

				if ( hitPosition3f.x < 0.0f || hitPosition3f.y < 0.0f || hitPosition3f.z < 0.0f ||
					 hitPosition3f.x > SizeX || hitPosition3f.y > SizeY || hitPosition3f.z > SizeZ )
				{
					distance = length;

					return BlockFace.Invalid;
				}

				hitPosition3f.z = 0.0f;
				IntVector3 blockHitPosition = new( (int)hitPosition3f.x, (int)hitPosition3f.y, (int)hitPosition3f.z );

				byte blockId = GetBlock( blockHitPosition );

				if ( blockId == 0 )
				{
					distance = distanceHit;
					hitPosition = blockHitPosition;
					hitPosition.z = -1;

					return BlockFace.Top;
				}
			}

			distance = length;

			return BlockFace.Invalid;
		}

		private void SetupChunks()
		{
			Chunks = new Chunk[NumChunksX * NumChunksY * NumChunksZ];

			for ( int x = 0; x < NumChunksX; ++x )
			{
				for ( int y = 0; y < NumChunksY; ++y )
				{
					for ( int z = 0; z < NumChunksZ; ++z )
					{
						var chunk = new Chunk( this, x, y, z );
						Chunks[chunk.Index] = chunk;
					}
				}
			}
		}

		private void SetBlockAtPosition( IntVector3 position, byte blockId )
		{
			Host.AssertServer();

			var chunkIndex = GetChunkIndex( position );
			var localPosition = ToLocalPosition( position );
			var chunk = Chunks[chunkIndex];
			var block = GetBlockType( blockId );

			chunk.SetBlock( localPosition, blockId );
			block.OnBlockAdded( chunk, position.x, position.y, position.z, (int)BlockFace.Top );

			var entityName = IsServer ? block.ServerEntity : block.ClientEntity;

			if ( !string.IsNullOrEmpty( entityName ) )
			{
				var entity = Library.Create<BlockEntity>( entityName );
				entity.BlockType = block;
				chunk.SetEntity( localPosition, entity );
			}
		}

		private async void ChunkFullUpdateTask()
		{
			while ( true )
			{
				try
				{
					await GameTask.Delay( 100 );

					foreach ( var chunk in Chunks )
					{
						if ( chunk.QueueUpdateBlockSlices )
						{
							chunk.UpdateBlockSlices();
							chunk.QueueUpdateBlockSlices = false;
							chunk.QueueRebuild = true;
						}
					}
				}
				catch ( TaskCanceledException e )
				{
					break;
				}
			}
		}
	}
}
