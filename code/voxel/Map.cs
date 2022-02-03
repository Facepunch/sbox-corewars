﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Map
	{
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

						Log.Info( $"[Client] Initializing block type {name} with id #{id}" );

						Current.BlockTypes.Add( name, id );
						Current.BlockData.Add( id, type );
					}
				}
			}

			Current.Init();
		}

		public Dictionary<byte, BlockType> BlockData { get; private set; } = new();
		public Dictionary<string, byte> BlockTypes { get; private set; } = new();
		public BlockAtlas BlockAtlas { get; private set; }
		public Texture LightMapTexture { get; private set; }
		public bool GreedyMeshing { get; private set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public int SizeX;
		public int SizeY;
		public int SizeZ;
		public int NumChunksX;
		public int NumChunksY;
		public int NumChunksZ;
		public Chunk[] Chunks;
		public byte[] LightMapData;

		private string BlockAtlasFileName { get; set; }
		private byte NextAvailableBlockId { get; set; }

		private Map()
		{
			BlockTypes[typeof( AirBlock ).Name] = NextAvailableBlockId;
			BlockData[NextAvailableBlockId] = new AirBlock( this );
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

			Log.Info( $"[Client] Initializing block type {type.GetType().Name} with id #{NextAvailableBlockId}" );

			BlockTypes[type.GetType().Name] = NextAvailableBlockId;
			BlockData[NextAvailableBlockId] = type;
			NextAvailableBlockId++;
		}

		public void ReceiveChunk( int index, byte[] data )
		{
			var chunk = Chunks[index];
			chunk.BlockTypes = data;
			chunk.Init();
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

		public byte GetSunlight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;
			var index = ToMapPosition( position );
			return LightMapData[index];
		}

		public void SetSunlight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return;
			var index = ToMapPosition( position );
			LightMapData[index] = value;
		}

		public byte GetTorchlight( IntVector3 position )
		{
			if ( !IsInside( position ) ) return 0;
			var index = ToMapPosition( position );
			return LightMapData[index];
		}

		public void SetTorchlight( IntVector3 position, byte value )
		{
			if ( !IsInside( position ) ) return;
			var index = ToMapPosition( position );
			LightMapData[index] = value;
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
					chunk.Init();
				}
			}
			else
			{
				LightMapData = new byte[SizeX * SizeY * SizeZ];
				LightMapTexture = Texture.CreateVolume( SizeX, SizeY, SizeZ )
					.WithFormat( ImageFormat.A8 )
					.WithData( LightMapData )
					.Finish();
			}

			Event.Register( this );
		}

		public bool SetBlockAndUpdate( IntVector3 position, byte blockId, bool forceUpdate = false )
		{
			var shouldBuild = false;
			var chunkIds = new HashSet<int>();

			if ( SetBlock( position, blockId ) || forceUpdate )
			{
				shouldBuild = true;

				var chunkIndex = GetChunkIndex( position );
				chunkIds.Add( chunkIndex );

				for ( int i = 0; i < 6; i++ )
				{
					if ( IsAdjacentEmpty( position, i ) )
					{
						var posInChunk = ToLocalPosition( position );
						Chunks[chunkIndex].UpdateBlockSlice( posInChunk, i );
						continue;
					}

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

		public int ToMapPosition( IntVector3 position )
		{
			return position.x * SizeY * SizeZ + position.y * SizeZ + position.z;
		}

		public int GetChunkIndex( IntVector3 position )
		{
			return (position.x / Chunk.ChunkSize) + (position.y / Chunk.ChunkSize) * NumChunksX + (position.z / Chunk.ChunkSize) * NumChunksX * NumChunksY;
		}

		public static IntVector3 ToLocalPosition( IntVector3 position )
		{
			return new IntVector3( position.x % Chunk.ChunkSize, position.y % Chunk.ChunkSize, position.z % Chunk.ChunkSize );
		}

		public static int GetOppositeDirection( int direction )
		{
			return direction + ((direction % 2 != 0) ? -1 : 1);
		}

		public void GeneratePerlin( byte groundBlockId )
		{
			for ( int x = 0; x < SizeX; ++x )
			{
				for ( int y = 0; y < SizeY; ++y )
				{
					int height = (int)((SizeZ / 2) * (Noise.Perlin( (x * 32) * 0.001f, (y * 32) * 0.001f, 0 ) + 0.5f) * 0.5f);
					if ( height <= 0 ) height = 1;
					if ( height > SizeZ ) height = SizeZ;

					for ( int z = 0; z < SizeZ; ++z )
					{
						SetBlockAtPosition( new IntVector3( x, y, z ), (byte)(z < height ? groundBlockId : 0) );
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

		public bool IsInside( IntVector3 position  )
		{
			if ( position.x < 0 || position.x >= SizeX ) return false;
			if ( position.y < 0 || position.y >= SizeY ) return false;
			if ( position.z < 0 || position.z >= SizeZ ) return false;

			return true;
		}

		public BlockInfo GetBlockInfo( IntVector3 position )
		{
			var positionInChunk = ToLocalPosition( position );

			return new BlockInfo
			{
				ChunkPosition = positionInChunk,
				ChunkIndex = GetChunkIndex( position ),
				IsValid = IsInside( position ),
				Position = position,
				BlockId = GetBlock( position )
			};
		}

		public bool SetBlock( IntVector3 position, byte blockId )
		{
			if ( !IsInside( position ) ) return false;

			var blockInfo = GetBlockInfo( position );
			var chunk = Chunks[blockInfo.ChunkIndex];
			var blockIndex = chunk.GetLocalPositionIndex( blockInfo.ChunkPosition );
			var currentBlockId = chunk.GetLocalIndexBlock( blockIndex );

			if ( blockId == currentBlockId ) return false;

			if ( (blockId != 0 && currentBlockId == 0) || (blockId == 0 && currentBlockId != 0) )
			{
				if ( IsClient )
					chunk.UpdateLighting( blockInfo, blockId );

				chunk.SetBlock( blockIndex, blockId );

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
			var blockPositionInChunk = ToLocalPosition( position );
			var chunk = Chunks[chunkIndex];

			chunk.SetBlock( blockPositionInChunk, blockId );
		}
	}
}
