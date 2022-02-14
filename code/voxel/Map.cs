﻿using Facepunch.CoreWars.Blocks;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Map : IValid
	{
		public struct ChunkBlockUpdate
		{
			public int x;
			public int y;
			public int z;
			public byte blockId;
			public int direction;
		}

		public delegate void OnInitializedCallback();
		public event OnInitializedCallback OnInitialized;

		public static Map Current { get; private set; }

		public static Map Create( int seed )
		{
			return new Map( seed );
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
					var seaLevel = reader.ReadInt32();
					var seed = reader.ReadInt32();
					var greedyMeshing = reader.ReadBoolean();

					Current = new Map( seed )
					{
						SeaLevel = seaLevel,
						GreedyMeshing = greedyMeshing
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

					var biomeCount = reader.ReadInt32();

					for ( var i = 0; i < biomeCount; i++ )
					{
						var biomeId = reader.ReadByte();
						var biomeLibraryId = reader.ReadInt32();
						var biome = Library.TryCreate<Biome>( biomeLibraryId );
						biome.Id = biomeId;
						biome.Map = Current;
						biome.Initialize();
						Current.BiomeLookup.Add( biomeId, biome );
						Current.Biomes.Add( biome );
						Log.Info( $"[Client] Initializing biome type {biome.Name}" );
					}
				}
			}

			Current.Init();
		}

		[ClientRpc]
		public static void ReceiveBlockUpdate( byte[] data )
		{
			var decompressed = CompressionHelper.Decompress( data );

			using ( var stream = new MemoryStream( decompressed ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					var count = reader.ReadInt32();
					var chunksToUpdate = new HashSet<Chunk>();

					for ( var i = 0; i < count; i++ )
					{
						var x = reader.ReadInt32();
						var y = reader.ReadInt32();
						var z = reader.ReadInt32();
						var blockId = reader.ReadByte();
						var direction = reader.ReadInt32();
						var position = new IntVector3( x, y, z );

						if ( Current.SetBlock( position, blockId, direction ) )
						{
							var chunk = Current.GetChunk( position );
							chunksToUpdate.Add( chunk );

							for ( int j = 0; j < 6; j++ )
							{
								var adjacentPosition = GetAdjacentPosition( position, j );
								var adjacentChunk = Current.GetChunk( adjacentPosition );

								if ( adjacentChunk.IsValid() )
								{
									chunksToUpdate.Add( adjacentChunk );
								}
							}
						}
					}

					foreach ( var chunk in chunksToUpdate )
					{
						chunk.QueueFullUpdate();
					}
				}
			}
		}

		[ClientRpc]
		public static void ReceiveDataUpdate( int x, int y, int z, byte[] data )
		{
			if ( Current == null ) return;

			var position = new IntVector3( x, y, z );
			var chunk = Current.GetChunk( position );

			if ( chunk.IsValid() )
			{
				chunk.DeserializeData( data );
			}
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

		public static Vector3 ToSourcePositionCenter( IntVector3 position )
		{
			var halfVoxelSize = Chunk.VoxelSize * 0.5f;
			return new Vector3(
				position.x * Chunk.VoxelSize + halfVoxelSize,
				position.y * Chunk.VoxelSize + halfVoxelSize,
				position.z * Chunk.VoxelSize + halfVoxelSize
			);
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
		public List<ChunkBlockUpdate> OutgoingBlockUpdates { get; private set; } = new();
		public Dictionary<byte, Biome> BiomeLookup { get; private set; } = new();
		public Dictionary<IntVector3, Chunk> Chunks { get; private set; } = new();
		public List<Biome> Biomes { get; private set; } = new();
		private Type ChunkGeneratorType { get; set; }
		public BlockAtlas BlockAtlas { get; private set; }
		public IntVector3 MaxSize { get; private set; }
		public bool GreedyMeshing { get; private set; }
		public bool Initialized { get; private set; }
		public int SeaLevel { get; private set; }
		public int Seed { get; private set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public int SizeX;
		public int SizeY;
		public int SizeZ;
		public int NumChunksX;
		public int NumChunksY;
		public int NumChunksZ;
		public FastNoiseLite CaveNoise;

		private List<Chunk> ChunkInitialUpdateList { get; set; } = new();
		private string BlockAtlasFileName { get; set; }
		private byte NextAvailableBlockId { get; set; }
		private byte NextAvailableBiomeId { get; set; }

		private BiomeSampler BiomeSampler;

		public bool IsInfinite => MaxSize == 0;
		public bool IsValid => true;

		private Map() { }

		private Map( int seed )
		{
			BlockTypes[typeof( AirBlock ).Name] = NextAvailableBlockId;
			BlockData[NextAvailableBlockId] = new AirBlock( this );

			CaveNoise = new( seed );
			CaveNoise.SetNoiseType( FastNoiseLite.NoiseType.OpenSimplex2 );
			CaveNoise.SetFractalType( FastNoiseLite.FractalType.FBm );
			CaveNoise.SetFractalOctaves( 2 );
			CaveNoise.SetFrequency( 1f / 128f );

			NextAvailableBlockId++;
			Current = this;
			Seed = seed;

			BiomeSampler = new BiomeSampler( this );
		}

		public void SetMaxSize( int x, int y, int z )
		{
			MaxSize = new IntVector3( x, y, z );
		}

		public Chunk GetOrCreateChunk( int x, int y, int z )
		{
			return GetOrCreateChunk( new IntVector3( x, y, z ) );
		}

		public Chunk GetOrCreateChunk( IntVector3 offset )
		{
			if ( Chunks.TryGetValue( offset, out var chunk ) )
			{
				return chunk;
			}

			chunk = new Chunk( this, offset.x, offset.y, offset.z );
			
			if ( IsServer && ChunkGeneratorType != null )
			{
				var generator = Library.Create<ChunkGenerator>( ChunkGeneratorType );
				generator.Setup( this, chunk );
				chunk.Generator = generator;
			}

			Chunks.Add( offset, chunk );

			lock ( ChunkInitialUpdateList )
			{
				ChunkInitialUpdateList.Add( chunk );
			}

			SizeX = Math.Max( SizeX, offset.x + Chunk.ChunkSize );
			SizeY = Math.Max( SizeY, offset.y + Chunk.ChunkSize );
			SizeZ = Math.Max( SizeZ, offset.z + Chunk.ChunkSize );

			return chunk;
		}

		public IntVector3 ToChunkOffset( IntVector3 position )
		{
			position.x = Math.Max( (position.x / Chunk.ChunkSize) * Chunk.ChunkSize, 0 );
			position.y = Math.Max( (position.y / Chunk.ChunkSize) * Chunk.ChunkSize, 0 );
			position.z = Math.Max( (position.z / Chunk.ChunkSize) * Chunk.ChunkSize, 0 );
			return position;
		}

		public Chunk GetChunk( IntVector3 position )
		{
			if ( position.x < 0 || position.y < 0 || position.z < 0 ) return null;

			position.x = (position.x / Chunk.ChunkSize) * Chunk.ChunkSize;
			position.y = (position.y / Chunk.ChunkSize) * Chunk.ChunkSize;
			position.z = (position.z / Chunk.ChunkSize) * Chunk.ChunkSize;

			if ( Chunks.TryGetValue( position, out var chunk ) )
			{
				return chunk;
			}

			return null;
		}

		public void SetChunkGenerator<T>() where T : ChunkGenerator
		{
			ChunkGeneratorType = typeof( T );
		}

		public T AddBiome<T>() where T : Biome
		{
			var biome = Library.Create<T>( typeof( T ) );
			biome.Id = NextAvailableBiomeId++;
			biome.Map = this;
			biome.Initialize();
			BiomeLookup.Add( biome.Id, biome );
			Biomes.Add( biome );
			return biome;
		}

		public void SetSeaLevel( int seaLevel )
		{
			SeaLevel = seaLevel;
		}

		public void RemoveChunk( Chunk chunk )
		{
			if ( chunk.IsValid() )
			{
				Chunks.Remove( chunk.Offset );
				chunk.Destroy();
			}
		}

		public void Send( Client client )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( SeaLevel );
					writer.Write( Seed );
					writer.Write( GreedyMeshing );
					writer.Write( BlockAtlasFileName );
					writer.Write( BlockData.Count - 1 );

					foreach ( var kv in BlockData )
					{
						if ( kv.Key == 0 ) continue;

						writer.Write( kv.Key );
						writer.Write( kv.Value.GetType().Name );
					}

					writer.Write( BiomeLookup.Count );

					foreach ( var kv in BiomeLookup )
					{
						var attribute = Library.GetAttribute( kv.Value.GetType() );
						writer.Write( kv.Key );
						writer.Write( attribute.Identifier );
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

		public void SetBlockOnServer( IntVector3 position, byte blockId, int direction = 0 )
		{
			Host.AssertServer();

			if ( SetBlockAndUpdate( position, blockId, direction ) )
			{
				OutgoingBlockUpdates.Add( new ChunkBlockUpdate
				{
					x = position.x,
					y = position.y,
					z = position.z,
					blockId = blockId,
					direction = direction
				} );
			}
		}

		public void SetBlockOnServer( int x, int y, int z, byte blockId, int direction = 0 )
		{
			SetBlockOnServer( new IntVector3( x, y, z ), blockId, direction );
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

		[ClientRpc]
		public static async void ReceiveChunks( byte[] data )
		{
			var decompressed = CompressionHelper.Decompress( data );

			using ( var stream = new MemoryStream( decompressed ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					var count = reader.ReadInt32();

					for ( var i = 0; i < count; i++ )
					{
						var x = reader.ReadInt32();
						var y = reader.ReadInt32();
						var z = reader.ReadInt32();

						var chunk = Current.GetOrCreateChunk( new IntVector3( x, y, z ) );
						
						chunk.Blocks = reader.ReadBytes( chunk.Blocks.Length );
						//chunk.LightMap.Deserialize( reader );
						chunk.DeserializeData( reader );
						chunk.Initialize();

						if ( i % 32 == 0 )
						{
							await GameTask.Delay( 1 );
						}
					}
				}
			}
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
		}

		public Voxel GetVoxel( IntVector3 position )
		{
			return GetVoxel( position.x, position.y, position.z );
		}

		public Voxel GetVoxel( int x, int y, int z )
		{
			var chunk = GetChunk( new IntVector3( x, y, z ) ) ;
			if ( !chunk.IsValid() ) return Voxel.Empty;
			return chunk.GetVoxel( x % Chunk.ChunkSize, y % Chunk.ChunkSize, z % Chunk.ChunkSize );
		}

		public T GetOrCreateData<T>( IntVector3 position ) where T : BlockData
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return null;

			var localPosition = ToLocalPosition( position );
			return chunk.GetOrCreateData<T>( localPosition );
		}

		public T GetData<T>( IntVector3 position ) where T : BlockData
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return null;

			var localPosition = ToLocalPosition( position );
			return chunk.GetData<T>( localPosition );
		}

		public byte GetSunLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return 0;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.GetSunLight( localPosition );
		}

		public bool SetSunLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return false;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.SetSunLight( localPosition, value );
		}

		public byte GetTorchLight( IntVector3 position, int channel )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return 0;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.GetTorchLight( localPosition, channel );
		}

		public bool SetTorchLight( IntVector3 position, int channel, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return false;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.SetTorchLight( localPosition, channel, value );
		}

		public byte GetRedTorchLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return 0;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.GetRedTorchLight( localPosition );
		}

		public bool SetRedTorchLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return false;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.SetRedTorchLight( localPosition, value );
		}

		public byte GetGreenTorchLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return 0;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.GetGreenTorchLight( localPosition );
		}

		public bool SetGreenTorchLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return false;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.SetGreenTorchLight( localPosition, value );
		}

		public byte GetBlueTorchLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return 0;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.GetGreenTorchLight( localPosition );
		}

		public bool SetBlueTorchLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return false;

			var localPosition = ToLocalPosition( position );
			return chunk.LightMap.SetGreenTorchLight( localPosition, value );
		}

		public void RemoveSunLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.RemoveSunLight( localPosition );
		}

		public void RemoveTorchLight( IntVector3 position, int channel )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.RemoveTorchLight( localPosition, channel );
		}

		public void RemoveRedTorchLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.RemoveRedTorchLight( localPosition );
		}

		public void RemoveGreenTorchLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.RemoveGreenTorchLight( localPosition );
		}

		public void RemoveBlueTorchLight( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.RemoveBlueTorchLight( localPosition );
		}

		public void AddSunLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.AddSunLight( localPosition, value );
		}

		public void AddTorchLight( IntVector3 position, int channel, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.AddTorchLight( localPosition, channel, value );
		}

		public void AddRedTorchLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.AddRedTorchLight( localPosition, value );
		}

		public void AddGreenTorchLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.AddGreenTorchLight( localPosition, value );
		}

		public void AddBlueTorchLight( IntVector3 position, byte value )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return;

			var localPosition = ToLocalPosition( position );
			chunk.LightMap.AddBlueTorchLight( localPosition, value );
		}

		public void Destroy()
		{
			foreach ( var kv in Chunks )
			{
				kv.Value.Destroy();
			}

			Event.Unregister( this );

			Chunks.Clear();
		}

		public void Init()
		{
			if ( Initialized ) return;

			if ( IsServer )
			{
				foreach ( var kv in Chunks )
				{
					kv.Value.Initialize();
				}
			}

			Initialized = true;
			OnInitialized?.Invoke();

			Event.Register( this );

			_ = GameTask.RunInThreadAsync( ChunkFullUpdateTask );
		}

		public bool SetBlockAndUpdate( IntVector3 position, byte blockId, int direction, bool forceUpdate = false )
		{
			var currentChunk = GetChunk( position );
			if ( !currentChunk.IsValid() ) return false;

			var shouldBuild = false;
			var chunksToUpdate = new HashSet<Chunk>();

			if ( SetBlock( position, blockId, direction ) || forceUpdate )
			{
				shouldBuild = true;
				chunksToUpdate.Add( currentChunk );

				for ( int i = 0; i < 6; i++ )
				{
					var adjacentPosition = GetAdjacentPosition( position, i );
					var adjacentChunk = GetChunk( adjacentPosition );

					if ( adjacentChunk.IsValid() )
					{
						chunksToUpdate.Add( adjacentChunk );
					}
				}
			}

			foreach ( var chunk in chunksToUpdate )
			{
				chunk.QueueFullUpdate();
			}

			return shouldBuild;
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

		public Biome GetBiomeAt( int x, int y )
		{
			return BiomeSampler.GetBiomeAt( x, y );
		}

		public byte GetBlock( IntVector3 position )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return 0;
			return chunk.GetMapPositionBlock( position );
		}

		public bool SetBlock( IntVector3 position, byte blockId, int direction )
		{
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return false;

			var localPosition = ToLocalPosition( position );
			var blockIndex = chunk.GetLocalPositionIndex( localPosition );
			var currentBlockId = chunk.GetLocalIndexBlock( blockIndex );

			if ( blockId == currentBlockId ) return false;

			if ( (blockId != 0 && currentBlockId == 0) || (blockId == 0 && currentBlockId != 0) )
			{
				var block = GetBlockType( blockId );

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

				var currentBlock = GetBlockType( currentBlockId );
				currentBlock.OnBlockRemoved( chunk, position.x, position.y, position.z );

				chunk.Data.Remove( localPosition );
				chunk.SetBlock( blockIndex, blockId );

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
			var chunk = GetChunk( position );
			if ( !chunk.IsValid() ) return true;
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

			var currentIterations = 0;

			while ( currentIterations < 1000 )
			{
				currentIterations++;

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
					axis = 0;
				else if ( lengthToNearestEdge.y < lengthToNearestEdge.x && lengthToNearestEdge.y < lengthToNearestEdge.z )
					axis = 1;
				else
					axis = 2;

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

		[Event.Tick.Server]
		private void ServerTick()
		{
			if ( OutgoingBlockUpdates.Count > 0 )
			{
				using ( var stream = new MemoryStream() )
				{
					using ( var writer = new BinaryWriter( stream ) )
					{
						writer.Write( OutgoingBlockUpdates.Count );
						
						foreach ( var update in OutgoingBlockUpdates )
						{
							writer.Write( update.x );
							writer.Write( update.y );
							writer.Write( update.z );
							writer.Write( update.blockId );
							writer.Write( update.direction );
						}

						var compressed = CompressionHelper.Compress( stream.ToArray() );
						ReceiveBlockUpdate( compressed.ToArray() );
					}
				}

				OutgoingBlockUpdates.Clear();
			}

			foreach ( var client in Client.All )
			{
				if ( client.Components.TryGet<ChunkViewer>( out var viewer ) )
				{
					viewer.Update();
				}
			}
		}

		private async void ChunkFullUpdateTask()
		{
			while ( true )
			{
				try
				{
					Chunk chunk;

					lock ( ChunkInitialUpdateList )
					{
						var chunks = ChunkInitialUpdateList.Where( c => c.Initialized && !c.HasDoneFirstFullUpdate );

						if ( IsClient )
						{
							var chunkSize = Chunk.ChunkSize;
							chunks = chunks.OrderBy( c => ToSourcePosition( c.Offset + new IntVector3( chunkSize / 2 ) ).Distance( Local.Pawn.Position ) );
						}

						chunk = chunks.FirstOrDefault();
					}

					if ( chunk.IsValid() )
					{
						_ = chunk.StartFirstFullUpdateTask();
					}

					if ( chunk != null )
					{
						lock ( ChunkInitialUpdateList )
						{
							ChunkInitialUpdateList.Remove( chunk );
						}
					}
				}
				catch ( TaskCanceledException e )
				{
					break;
				}
				catch ( Exception e )
				{
					Log.Error( e );
				}
			}
		}
	}
}
