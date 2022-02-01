using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Map : BaseNetworkable
	{
		[Net] public Dictionary<byte, BlockType> BlockTypes { get; private set; } = new();
		[Net] public int SizeX { get; private set; }
		[Net] public int SizeY { get; private set; }
		[Net] public int SizeZ { get; private set; }

		private int _numChunksX;
		private int _numChunksY;
		private int _numChunksZ;

		public int NumChunksX => _numChunksX;
		public int NumChunksY => _numChunksY;
		public int NumChunksZ => _numChunksZ;

		public Chunk[] Chunks { get; private set; }

		public void AddBlockType( BlockType type )
		{
			Host.AssertServer();
			BlockTypes[type.BlockId] = type;
		}

		public void ReceiveChunk( int index, byte[] data )
		{
			var chunk = Chunks[index];
			chunk.BlockTypes = data;
			chunk.UpdateBlockSlices();
			chunk.Build();
		}

		public void SetSize( int sizeX, int sizeY, int sizeZ )
		{
			SizeX = sizeX;
			SizeY = sizeY;
			SizeZ = sizeZ;

			_numChunksX = SizeX / Chunk.ChunkSize;
			_numChunksY = SizeY / Chunk.ChunkSize;
			_numChunksZ = SizeZ / Chunk.ChunkSize;
			
			SetupChunks();
		}

		public void Init()
		{
			_numChunksX = SizeX / Chunk.ChunkSize;
			_numChunksY = SizeY / Chunk.ChunkSize;
			_numChunksZ = SizeZ / Chunk.ChunkSize;

			if ( Chunks == null )
			{
				SetupChunks();
			}

			foreach ( var chunk in Chunks )
			{
				if ( chunk == null )
					continue;

				chunk.Map = this;
				chunk.Init();
			}
		}

		public bool SetBlockAndUpdate( IntVector3 position, byte blockType, bool forceUpdate = false )
		{
			var shouldBuild = false;
			var chunkids = new HashSet<int>();

			if ( SetBlock( position, blockType ) || forceUpdate )
			{
				var chunkIndex = GetBlockChunkIndexAtPosition( position );

				chunkids.Add( chunkIndex );

				shouldBuild = true;

				for ( int i = 0; i < 6; i++ )
				{
					if ( IsAdjacentBlockEmpty( position, i ) )
					{
						var posInChunk = GetBlockPositionInChunk( position );
						Chunks[chunkIndex].UpdateBlockSlice( posInChunk, i );

						continue;
					}

					var adjacentPos = GetAdjacentBlockPosition( position, i );
					var adjadentChunkIndex = GetBlockChunkIndexAtPosition( adjacentPos );
					var adjacentPosInChunk = GetBlockPositionInChunk( adjacentPos );

					chunkids.Add( adjadentChunkIndex );

					Chunks[adjadentChunkIndex].UpdateBlockSlice( adjacentPosInChunk, GetOppositeDirection( i ) );
				}
			}

			foreach ( var chunkid in chunkids )
			{
				Chunks[chunkid].Build();
			}

			return shouldBuild;
		}

		public int GetBlockChunkIndexAtPosition( IntVector3 position )
		{
			return (position.x / Chunk.ChunkSize) + (position.y / Chunk.ChunkSize) * _numChunksX + (position.z / Chunk.ChunkSize) * _numChunksX * _numChunksY;
		}

		public static IntVector3 GetBlockPositionInChunk( IntVector3 position )
		{
			return new IntVector3( position.x % Chunk.ChunkSize, position.y % Chunk.ChunkSize, position.z % Chunk.ChunkSize );
		}

		public static int GetOppositeDirection( int direction )
		{
			return direction + ((direction % 2 != 0) ? -1 : 1);
		}

		public void GeneratePerlin()
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
						SetBlockTypeAtPosition( new IntVector3( x, y, z ), (byte)(z < height ? 1 : 0) );
					}
				}
			}
		}

		public byte GetBlockTypeAtPosition( IntVector3 position )
		{
			var chunkIndex = GetBlockChunkIndexAtPosition( position );
			var blockPositionInChunk = GetBlockPositionInChunk( position );
			var chunk = Chunks[chunkIndex];

			return chunk.GetBlockTypeAtPosition( blockPositionInChunk );
		}

		public bool SetBlock( IntVector3 position, byte blockType )
		{
			if ( position.x < 0 || position.x >= SizeX ) return false;
			if ( position.y < 0 || position.y >= SizeY ) return false;
			if ( position.z < 0 || position.z >= SizeZ ) return false;

			var chunkIndex = GetBlockChunkIndexAtPosition( position );
			var blockPositionInChunk = GetBlockPositionInChunk( position );
			int blockindex = Chunk.GetBlockIndexAtPosition( blockPositionInChunk );
			var chunk = Chunks[chunkIndex];
			int currentBlockType = chunk.GetBlockTypeAtIndex( blockindex );

			if ( blockType == currentBlockType )
			{
				return false;
			}

			if ( (blockType != 0 && currentBlockType == 0) || (blockType == 0 && currentBlockType != 0) )
			{
				chunk.SetBlockTypeAtIndex( blockindex, blockType );

				return true;
			}

			return false;
		}

		public static IntVector3 GetAdjacentBlockPosition( IntVector3 position, int side )
		{
			return position + Chunk.BlockDirections[side];
		}

		public bool IsAdjacentBlockEmpty( IntVector3 position, int side )
		{
			return IsBlockEmpty( GetAdjacentBlockPosition( position, side ) );
		}

		public bool IsBlockEmpty( IntVector3 position )
		{
			if ( position.x < 0 || position.x >= SizeX ||
				 position.y < 0 || position.y >= SizeY )
			{
				return true;
			}

			if ( position.z < 0 || position.z >= SizeZ )
			{
				return true;
			}

			if ( position.z >= SizeZ )
			{
				return true;
			}

			var chunkIndex = GetBlockChunkIndexAtPosition( position );
			var blockPositionInChunk = GetBlockPositionInChunk( position );
			var chunk = Chunks[chunkIndex];

			return chunk.GetBlockTypeAtPosition( blockPositionInChunk ) == 0;
		}

		public BlockFace GetBlockInDirection( Vector3 position, Vector3 direction, float length, out IntVector3 hitPosition, out float distance )
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

				byte blockType = GetBlockTypeAtPosition( position3i );

				if ( blockType != 0 )
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

				byte blockType = GetBlockTypeAtPosition( blockHitPosition );

				if ( blockType == 0 )
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
			Chunks = new Chunk[_numChunksX * _numChunksY * _numChunksZ];

			for ( int x = 0; x < _numChunksX; ++x )
			{
				for ( int y = 0; y < _numChunksY; ++y )
				{
					for ( int z = 0; z < _numChunksZ; ++z )
					{
						var chunk = new Chunk( this, x, y, z );
						Chunks[chunk.Index] = chunk;
					}
				}
			}
		}

		private void SetBlockTypeAtPosition( IntVector3 position, byte blockType )
		{
			Host.AssertServer();

			var chunkIndex = GetBlockChunkIndexAtPosition( position );
			var blockPositionInChunk = GetBlockPositionInChunk( position );
			var chunk = Chunks[chunkIndex];

			chunk.SetBlockTypeAtPosition( blockPositionInChunk, blockType );
		}
	}
}
