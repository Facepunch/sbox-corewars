﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
		public bool Initialized { get; private set; }

		public ChunkLightMap LightMap { get; set; }
		public byte[] BlockTypes;
		public IntVector3 Offset;
		public int Index;
		public Map Map;

		private static readonly BlockFaceData[] BlockFaceMask = new BlockFaceData[ChunkSize * ChunkSize * ChunkSize];
		private readonly ChunkSlice[] Slices = new ChunkSlice[ChunkSize * 6];
		private SceneObject TranslucentSceneObject { get; set; }
		private SceneObject OpaqueSceneObject { get; set; }
		private Model TranslucentModel { get; set; }
		private Model OpaqueModel { get; set; }
		private Mesh TranslucentMesh { get; set; }
		private Mesh OpaqueMesh { get; set; }

		public Chunk() { }

		public Chunk( Map map, int x, int y, int z )
		{
			BlockTypes = new byte[ChunkSize * ChunkSize * ChunkSize];
			LightMap = new ChunkLightMap( this, map );
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

			var translucentModelBuilder = new ModelBuilder();
			var opaqueModelBuilder = new ModelBuilder();

			if ( Host.IsClient )
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

			if ( Host.IsClient )
			{
				translucentModelBuilder.AddMesh( TranslucentMesh );
				opaqueModelBuilder.AddMesh( OpaqueMesh );
			}

			TranslucentModel = translucentModelBuilder.Create();
			OpaqueModel = opaqueModelBuilder.Create();

			if ( Host.IsClient )
			{
				var transform = new Transform( Offset * (float)VoxelSize );

				OpaqueSceneObject = new SceneObject( OpaqueModel, transform );
				OpaqueSceneObject.SetValue( "VoxelSize", VoxelSize );
				OpaqueSceneObject.SetValue( "LightMap", LightMap.TorchLightTexture );
				OpaqueSceneObject.SetValue( "SunLight", LightMap.SunLightTexture );

				TranslucentSceneObject = new SceneObject( TranslucentModel, transform );
				TranslucentSceneObject.SetValue( "VoxelSize", VoxelSize );
				TranslucentSceneObject.SetValue( "LightMap", LightMap.TorchLightTexture );
				TranslucentSceneObject.SetValue( "SunLight", LightMap.SunLightTexture );

				UpdateAdjacents( true );
			}

			Event.Register( this );

			Initialized = true;
		}

		public async void FullUpdate()
		{
			await UpdateBlockSlices();
			Build();
		}

		public void UpdateAdjacents( bool recurseNeighbours = false )
		{
			UpdateAdjacents( TranslucentSceneObject, recurseNeighbours );
			UpdateAdjacents( OpaqueSceneObject, recurseNeighbours );
		}

		public void UpdateAdjacents( SceneObject sceneObject, bool recurseNeighbours = false )
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

			int maxChunkSize = Map.Chunks.Length;

			if ( westChunk >= 0 && westChunk < maxChunkSize )
			{
				var neighbour = Map.Chunks[westChunk];

				if ( neighbour != null && neighbour.Initialized )
				{
					sceneObject.SetValue( "LightMapWest", neighbour.LightMap.TorchLightTexture );
					sceneObject.SetValue( "SunLightWest", neighbour.LightMap.SunLightTexture );
					if ( recurseNeighbours ) neighbour.UpdateAdjacents();
				}
			}

			if ( southChunk >= 0 && southChunk < maxChunkSize )
			{
				var neighbour = Map.Chunks[southChunk];

				if ( neighbour != null && neighbour.Initialized )
				{
					sceneObject.SetValue( "LightMapSouth", neighbour.LightMap.TorchLightTexture );
					sceneObject.SetValue( "SunLightSouth", neighbour.LightMap.SunLightTexture );
					if ( recurseNeighbours ) neighbour.UpdateAdjacents();
				}
			}

			if ( eastChunk >= 0 && eastChunk < maxChunkSize )
			{
				var neighbour = Map.Chunks[eastChunk];

				if ( neighbour != null && neighbour.Initialized )
				{
					sceneObject.SetValue( "LightMapEast", neighbour.LightMap.TorchLightTexture );
					sceneObject.SetValue( "SunLightEast", neighbour.LightMap.SunLightTexture );
					if ( recurseNeighbours ) neighbour.UpdateAdjacents();
				}
			}

			if ( northChunk >= 0 && northChunk < maxChunkSize )
			{
				var neighbour = Map.Chunks[northChunk];

				if ( neighbour != null && neighbour.Initialized )
				{
					sceneObject.SetValue( "LightMapNorth", neighbour.LightMap.TorchLightTexture );
					sceneObject.SetValue( "SunLightNorth", neighbour.LightMap.SunLightTexture );
					if ( recurseNeighbours ) neighbour.UpdateAdjacents();
				}
			}
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
			return BlockTypes[index];
		}

		public byte GetLocalPositionBlock( int x, int y, int z )
		{
			return BlockTypes[GetLocalPositionIndex( x, y, z )];
		}

		public byte GetLocalPositionBlock( IntVector3 position )
		{
			return BlockTypes[GetLocalPositionIndex( position )];
		}

		public byte GetLocalIndexBlock( int index )
		{
			return BlockTypes[index];
		}

		public IntVector3 ToMapPosition( IntVector3 position )
		{
			return Offset + position;
		}

		public void PropagateSunlight()
		{
			var positionAbove = Offset + (BlockDirections[0] * ChunkSize);

			if ( Map.IsInside( positionAbove ) )
			{
				var chunkAboveIndex = Map.GetChunkIndex( positionAbove );
				var chunkAbove = Map.Chunks[chunkAboveIndex];

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
			else
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
			}
		}

		public void SetBlock( IntVector3 position, byte blockId )
		{
			BlockTypes[GetLocalPositionIndex( position )] = blockId;
		}

		public void SetBlock( int index, byte blockId )
		{
			BlockTypes[index] = blockId;
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

			foreach ( var slice in Slices )
			{
				if ( slice == null )
					continue;

				slice.Body = null;
			}

			Event.Unregister( this );
		}

		public void BuildMeshAndCollision()
		{
			if ( !OpaqueMesh.IsValid )
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

				slice.CollisionVertices.Add( new Vector3( (x + vOffset.x) + Offset.x, (y + vOffset.y) + Offset.y, (z + vOffset.z) + Offset.z ) * VoxelSize );
				slice.CollisionIndices.Add( collisionIndex + i );
			}
		}

		BlockFaceData GetBlockFace( IntVector3 position, int side )
		{
			var p = Offset + position;
			var blockEmpty = Map.IsEmpty( p );
			var blockId = blockEmpty ? (byte)0 : Host.IsServer ? (byte)1 : Map.GetBlock( p );

			var face = new BlockFaceData
			{
				BlockIndex = GetLocalPositionIndex( position ),
				Side = (byte)side,
				Culled = blockId == 0,
				Type = blockId,
			};

			var adjacentBlockPosition = Map.GetAdjacentPosition( p, side );
			var adjacentBlockId = Map.GetBlock( adjacentBlockPosition );
			var adjacentBlock = Map.GetBlockType( adjacentBlockId );

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
							BlockFaceMask[n].Side, BlockFaceMask[n].Type );

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

		public async Task UpdateBlockSlices()
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
									BlockFaceMask[n].Side, BlockFaceMask[n].Type );
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

		[Event.Tick.Client]
		private void ClientTick()
		{
			LightMap.UpdateTorchLight();
			LightMap.UpdateSunLight();
		}
	}
}
