﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Voxel
{
	public struct LightNode
	{
		public int ChunkIndex;
		public int BlockIndex;
	}

	public partial class Chunk
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
			public int LightLevel;
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

		public HashSet<SliceUpdate> PendingSliceUpdates { get; set; } = new();
		public Queue<LightRemoveNode> LightRemoveQueue { get; private set; } = new();
		public Queue<IntVector3> LightAddQueue { get; private set; } = new();
		public bool Initialized { get; private set; }

		public char[] LightMap;
		public byte[] BlockTypes;
		public IntVector3 Offset;
		public int Index;
		public Map Map;

		private static readonly BlockFaceData[] BlockFaceMask = new BlockFaceData[ChunkSize * ChunkSize * ChunkSize];
		private readonly ChunkSlice[] Slices = new ChunkSlice[ChunkSize * 6];
		private SceneObject SceneObject { get; set; }
		private Model Model { get; set; }
		private Mesh Mesh { get; set; }

		public Chunk() { }

		public Chunk( Map map, int x, int y, int z )
		{
			LightMap = new char[ChunkSize * ChunkSize * ChunkSize];
			BlockTypes = new byte[ChunkSize * ChunkSize * ChunkSize];
			Offset = new IntVector3( x * ChunkSize, y * ChunkSize, z * ChunkSize );
			Index = x + y * map.NumChunksX + z * map.NumChunksX * map.NumChunksY;
			Map = map;
		}

		public async void Init()
		{
			if ( Initialized )
				return;

			for ( int i = 0; i < Slices.Length; ++i )
			{
				Slices[i] = new ChunkSlice();
			}

			await UpdateBlockSlices();

			foreach ( var update in PendingSliceUpdates )
			{
				UpdateBlockSlice( update.Position, update.Direction );
			}

			PendingSliceUpdates.Clear();

			var modelBuilder = new ModelBuilder();

			if ( Host.IsClient )
			{
				var material = Material.Load( "materials/corewars/voxel.vmat" );
				Mesh = new Mesh( material );

				var boundsMin = Vector3.Zero;
				var boundsMax = boundsMin + (ChunkSize * VoxelSize);
				Mesh.SetBounds( boundsMin, boundsMax );
			}

			Build();

			if ( Host.IsClient )
			{
				modelBuilder.AddMesh( Mesh );
			}

			Model = modelBuilder.Create();

			if ( Host.IsClient )
			{
				var transform = new Transform( Offset * (float)VoxelSize );
				SceneObject = new SceneObject( Model, transform );
				SceneObject.SetValue( "VoxelSize", VoxelSize );
			}

			Initialized = true;
		}

		public int GetSunlight( IntVector3 position )
		{
			var index = GetLightMapIndex( position );
			return (LightMap[index] >> 4) & 0xF;
		}

		public int GetSunlight( int index )
		{
			return (LightMap[index] >> 4) & 0xF;
		}

		public void SetSunlight( IntVector3 position, int value )
		{
			var index = GetLightMapIndex( position );
			LightMap[index] = (char)((LightMap[index] & 0xF) | (value << 4));
		}

		public int GetTorchlight( IntVector3 position )
		{
			var index = GetLightMapIndex( position );
			return LightMap[index] & 0xF;
		}

		public int GetTorchlight( int index )
		{
			return LightMap[index] & 0xF;
		}

		public void SetTorchlight( IntVector3 position, int value )
		{
			var index = GetLightMapIndex( position );
			LightMap[index] = (char)((LightMap[index] & 0xF0) | value);
		}

		public async void FullUpdate()
		{
			await UpdateBlockSlices();
			Build();
		}

		public void Build()
		{
			if ( Host.IsServer )
			{
				BuildCollision();
				return;
			}

			BuildMeshAndCollision();
		}

		public static int GetLightMapIndex( IntVector3 position )
		{
			return position.x + position.y * ChunkSize + position.z * ChunkSize * ChunkSize;
		}

		public static int GetBlockIndex( IntVector3 position )
		{
			return position.x + position.y * ChunkSize + position.z * ChunkSize * ChunkSize;
		}

		public byte GetBlockByPosition( IntVector3 position )
		{
			return BlockTypes[GetBlockIndex( position )];
		}

		public byte GetBlockByIndex( int index )
		{
			return BlockTypes[index];
		}

		public IntVector3 ToMapPosition( IntVector3 position )
		{
			return Offset + position;
		}

		public void UpdateLighting( BlockInfo blockInfo, byte newBlockId, List<IntVector3> affectedBlocks )
		{
			affectedBlocks.Add( blockInfo.Position );

			var block = Map.GetBlockType( newBlockId );
			int oldValue = GetTorchlight( blockInfo.ChunkPosition );

			SetTorchlight( blockInfo.ChunkPosition, 0 );

			LightRemoveQueue.Enqueue( new LightRemoveNode
			{
				Position = blockInfo.Position,
				Value = oldValue
			} );

			while ( LightRemoveQueue.Count > 0 )
			{
				var node = LightRemoveQueue.Dequeue();

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentBlockPosition( node.Position, i );
					var neighbourBlockInfo = Map.GetBlockInfo( neighbourPosition );
					if ( !neighbourBlockInfo.IsValid ) continue;

					var neighbourChunk = Map.Chunks[neighbourBlockInfo.ChunkIndex];
					var lightLevel = neighbourChunk.GetTorchlight( neighbourBlockInfo.ChunkPosition );

					if ( lightLevel != 0 && lightLevel < node.Value )
					{
						neighbourChunk.SetTorchlight( neighbourBlockInfo.ChunkPosition, 0 );

						LightRemoveQueue.Enqueue( new LightRemoveNode
						{
							Position = neighbourPosition,
							Value = node.Value
						} );

						affectedBlocks.Add( neighbourPosition );
					}
					else if ( lightLevel >= node.Value )
					{
						LightAddQueue.Enqueue( neighbourPosition );
					}
				}
			}

			if ( block != null && block.LightLevel > 0 )
			{
				SetTorchlight( blockInfo.ChunkPosition, block.LightLevel );
				LightAddQueue.Enqueue( blockInfo.Position );
			}

			while ( LightAddQueue.Count > 0 )
			{
				var nodePosition = LightAddQueue.Dequeue();
				var nodeChunkIndex = Map.GetBlockChunkIndex( nodePosition );
				var nodeChunk = Map.Chunks[nodeChunkIndex];
				var lightLevel = nodeChunk.GetTorchlight( Map.GetBlockPositionInChunk( nodePosition ) );

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentBlockPosition( nodePosition, i );
					var neighbourBlockInfo = Map.GetBlockInfo( neighbourPosition );
					if ( !neighbourBlockInfo.IsValid ) continue;

					var neighbourBlock = Map.GetBlockType( neighbourBlockInfo.BlockId );
					var neighbourChunk = Map.Chunks[neighbourBlockInfo.ChunkIndex];

					if ( neighbourChunk.GetTorchlight( neighbourBlockInfo.ChunkPosition ) + 2 <= lightLevel )
					{
						if ( neighbourBlock.IsTranslucent )
						{
							// For testing purposes move this up outside of this scope.
							neighbourChunk.SetTorchlight( neighbourBlockInfo.ChunkPosition, lightLevel - 1 );
							LightAddQueue.Enqueue( neighbourPosition );
						}
						else
						{
							affectedBlocks.Add( neighbourPosition );
						}
					}
				}
			}
		}

		public void SetBlock( IntVector3 position, byte blockId )
		{
			BlockTypes[GetBlockIndex( position )] = blockId;
		}

		public void SetBlock( int index, byte blockId )
		{
			BlockTypes[index] = blockId;
		}

		public void Destroy()
		{
			if ( SceneObject != null )
			{
				SceneObject.Delete();
				SceneObject = null;
			}

			foreach ( var slice in Slices )
			{
				if ( slice == null )
					continue;

				slice.Body = null;
			}
		}

		public void BuildMeshAndCollision( bool keepCollisionMesh = false )
		{
			if ( !Mesh.IsValid )
				return;

			int vertexCount = 0;
			foreach ( var slice in Slices )
			{
				vertexCount += slice.Vertices.Count;
			}

			if ( Mesh.HasVertexBuffer )
				Mesh.SetVertexBufferSize( vertexCount );
			else
				Mesh.CreateVertexBuffer<BlockVertex>( Math.Max( 1, vertexCount ), BlockVertex.Layout );

			vertexCount = 0;

			foreach ( var slice in Slices )
			{
				if ( !keepCollisionMesh && slice.IsDirty )
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

				if ( slice.Vertices.Count == 0 )
					continue;

				Mesh.SetVertexBufferData( slice.Vertices, vertexCount );
				vertexCount += slice.Vertices.Count;
			}

			Mesh.SetVertexRange( 0, vertexCount );
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

		private void AddQuad( ChunkSlice slice, int x, int y, int z, int width, int height, int widthAxis, int heightAxis, int face, byte blockId, int brightness )
		{
			var block = Map.GetBlockType( blockId );

			if ( block == null )
				throw new Exception( $"Unable to find a block type registered with the id: {blockId}!" );

			var textureId = block.GetTextureId( (BlockFace)face );
			var normal = (byte)face;
			var faceData = (uint)((textureId & 31) << 18 | (brightness & 15) << 23 | (normal & 7) << 27);
			var collisionIndex = slice.CollisionIndices.Count;

			for ( int i = 0; i < 6; ++i )
			{
				int vi = BlockIndices[(face * 6) + i];
				var vOffset = BlockVertices[vi];

				vOffset[widthAxis] *= width;
				vOffset[heightAxis] *= height;

				slice.Vertices.Add( new BlockVertex( (uint)(x + vOffset.x), (uint)(y + vOffset.y), (uint)(z + vOffset.z), faceData ) );

				slice.CollisionVertices.Add( new Vector3( (x + vOffset.x) + Offset.x, (y + vOffset.y) + Offset.y, (z + vOffset.z) + Offset.z ) * VoxelSize );
				slice.CollisionIndices.Add( collisionIndex + i );
			}
		}

		BlockFaceData GetBlockFace( IntVector3 position, int side )
		{
			var p = Offset + position;
			var blockEmpty = Map.IsBlockEmpty( p );
			var blockId = blockEmpty ? (byte)0 : Host.IsServer ? (byte)1 : Map.GetBlock( p );

			var face = new BlockFaceData
			{
				BlockIndex = GetBlockIndex( position ),
				LightLevel = Map.GetTorchlight( p ),
				Side = (byte)side,
				Culled = blockId == 0,
				Type = blockId,
			};

			var adjacentBlockPosition = Map.GetAdjacentBlockPosition( p, side );
			var adjacentBlockId = Map.GetBlock( adjacentBlockPosition );
			var adjacentBlock = Map.GetBlockType( adjacentBlockId );

			if ( Map.IsInMap( adjacentBlockPosition ) )
				face.LightLevel = Map.GetTorchlight( adjacentBlockPosition );

			if ( !face.Culled && (adjacentBlock != null && !adjacentBlock.IsTranslucent) )
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

			int vertexOffset = 0;
			int axis = BlockDirectionAxis[direction];
			int sliceIndex = GetSliceIndex( position[axis], direction );
			var slice = Slices[sliceIndex];

			if ( slice.IsDirty ) return;

			slice.IsDirty = true;
			slice.Vertices.Clear();
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
						BlockFaceMask[n].Culled = true;
					}
					else
					{
						BlockFaceMask[n] = faceA;

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
					if ( BlockFaceMask[n].Culled )
					{
						i++;
						n++;

						continue;
					}

					int faceWidth;
					int faceHeight;

					if ( Map.GreedyMeshing )
					{
						for ( faceWidth = 1; i + faceWidth < ChunkSize && !BlockFaceMask[n + faceWidth].Culled && BlockFaceMask[n + faceWidth].Equals( BlockFaceMask[n] ); faceWidth++ )
						{

						}

						bool done = false;

						for ( faceHeight = 1; j + faceHeight < ChunkSize; faceHeight++ )
						{
							for ( int k = 0; k < faceWidth; k++ )
							{
								var maskFace = BlockFaceMask[n + k + faceHeight * ChunkSize];

								if ( maskFace.Culled || !maskFace.Equals( BlockFaceMask[n] ) )
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

					if ( !BlockFaceMask[n].Culled )
					{
						blockPosition[uAxis] = i;
						blockPosition[vAxis] = j;

						AddQuad( slice,
							blockPosition.x, blockPosition.y, blockPosition.z,
							faceWidth, faceHeight, uAxis, vAxis,
							BlockFaceMask[n].Side, BlockFaceMask[n].Type, BlockFaceMask[n].LightLevel );

						vertexOffset += 6;
					}

					for ( int l = 0; l < faceHeight; ++l )
					{
						for ( int k = 0; k < faceWidth; ++k )
						{
							BlockFaceMask[n + k + l * ChunkSize].Culled = true;
						}
					}

					i += faceWidth;
					n += faceWidth;
				}
			}
		}

		public async Task UpdateBlockSlices( bool keepCollisionMesh = false )
		{
			IntVector3 blockPosition;
			IntVector3 blockOffset;

			BlockFaceData faceA;
			BlockFaceData faceB;

			for ( int faceSide = 0; faceSide < 6; faceSide++ )
			{
				await GameTask.Delay( 20 );

				int axis = BlockDirectionAxis[faceSide];

				int uAxis = (axis + 1) % 3;
				int vAxis = (axis + 2) % 3;

				blockPosition = new IntVector3( 0, 0, 0 );
				blockOffset = BlockDirections[faceSide];

				for ( blockPosition[axis] = 0; blockPosition[axis] < ChunkSize; blockPosition[axis]++ )
				{
					int n = 0;
					bool maskEmpty = true;

					int sliceIndex = GetSliceIndex( blockPosition[axis], faceSide );
					var slice = Slices[sliceIndex];
					slice.IsDirty = true;
					slice.Vertices.Clear();

					if ( !keepCollisionMesh )
					{
						slice.CollisionVertices.Clear();
						slice.CollisionIndices.Clear();
					}

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
								BlockFaceMask[n].Culled = true;
							}
							else
							{
								BlockFaceMask[n] = faceA;

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
							if ( BlockFaceMask[n].Culled )
							{
								i++;
								n++;

								continue;
							}

							int faceWidth;
							int faceHeight;

							if ( Map.GreedyMeshing )
							{
								for ( faceWidth = 1; i + faceWidth < ChunkSize && !BlockFaceMask[n + faceWidth].Culled && BlockFaceMask[n + faceWidth].Equals( BlockFaceMask[n] ); faceWidth++ )
								{

								}

								bool done = false;

								for ( faceHeight = 1; j + faceHeight < ChunkSize; faceHeight++ )
								{
									for ( int k = 0; k < faceWidth; k++ )
									{
										var maskFace = BlockFaceMask[n + k + faceHeight * ChunkSize];

										if ( maskFace.Culled || !maskFace.Equals( BlockFaceMask[n] ) )
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

							if ( !BlockFaceMask[n].Culled )
							{
								blockPosition[uAxis] = i;
								blockPosition[vAxis] = j;

								AddQuad( slice,
									blockPosition.x, blockPosition.y, blockPosition.z,
									faceWidth, faceHeight, uAxis, vAxis,
									BlockFaceMask[n].Side, BlockFaceMask[n].Type, BlockFaceMask[n].LightLevel );
							}

							for ( int l = 0; l < faceHeight; ++l )
							{
								for ( int k = 0; k < faceWidth; ++k )
								{
									BlockFaceMask[n + k + l * ChunkSize].Culled = true;
								}
							}

							i += faceWidth;
							n += faceWidth;
						}
					}
				}
			}
		}
	}
}
