using Sandbox;
using System;

namespace Facepunch.CoreWars.Voxel
{
	public partial class Chunk : Entity
	{
		private struct BlockFace
		{
			public bool Culled;
			public byte Type;
			public byte Side;

			public bool Equals( BlockFace face )
			{
				return face.Culled == Culled && face.Type == Type;
			}
		};

		public static readonly int ChunkSize = 32;
		public static readonly int VoxelSize = 48;

		public ChunkData Data { get; set; }
		public Map Map { get; set; }

		private IntVector3 Offset => Data.Offset;

		private static readonly BlockFace[] BlockFaceMask = new BlockFace[ChunkSize * ChunkSize * ChunkSize];
		private readonly ChunkSlice[] Slices = new ChunkSlice[ChunkSize * 6];
		private SceneObject SceneObject;
		private bool Initialized;
		private Model Model;
		private Mesh Mesh;

		public Chunk() { }

		public Chunk( Map map, ChunkData data )
		{
			Map = map;
			Data = data;
			Transmit = TransmitType.Always;
		}

		[ClientRpc]
		public void UpdateAll( int x, int y, int z, byte[] data )
		{
			Data = new();
			Data.Offset = new IntVector3( x, y, z );
			Data.BlockTypes = data;

			Log.Info( $"Received all bytes for chunk{x},{y},{z} ({data.Length / 1024}kb)" );
		}

		[Event.Tick.Client]
		public void InitTick()
		{
			if ( Initialized )
				return;

			if ( Data != null )
			{
				Init();
			}
		}

		public void Init()
		{
			if ( Initialized )
				return;

			if ( Data == null )
				return;

			for ( int i = 0; i < Slices.Length; ++i )
			{
				Slices[i] = new ChunkSlice();
			}

			UpdateBlockSlices();

			var modelBuilder = new ModelBuilder();

			if ( IsClient )
			{
				var material = Material.Load( "materials/corewars/voxel.vmat" );
				Mesh = new Mesh( material );

				var boundsMin = Vector3.Zero;
				var boundsMax = boundsMin + (ChunkSize * VoxelSize);
				Mesh.SetBounds( boundsMin, boundsMax );
			}

			Build();

			if ( IsClient )
			{
				modelBuilder.AddMesh( Mesh );
			}

			Model = modelBuilder.Create();

			if ( IsClient )
			{
				var transform = new Transform( Offset * (float)VoxelSize );
				SceneObject = new SceneObject( Model, transform );
				SceneObject.SetValue( "VoxelSize", VoxelSize );
			}

			Initialized = true;
		}

		public void Build()
		{
			if ( IsServer )
				BuildCollision();
			else
				BuildMeshAndCollision();
		}

		public static int GetBlockIndexAtPosition( IntVector3 position )
		{
			return position.x + position.y * ChunkSize + position.z * ChunkSize * ChunkSize;
		}

		public byte GetBlockTypeAtPosition( IntVector3 position )
		{
			return Data.GetBlockTypeAtPosition( position );
		}

		public byte GetBlockTypeAtIndex( int index )
		{
			return Data.GetBlockTypeAtIndex( index );
		}

		public void SetBlockTypeAtPosition( IntVector3 position, byte blockType )
		{
			Data.SetBlockTypeAtPosition( position, blockType );
		}

		public void SetBlockTypeAtIndex( int index, byte blockType )
		{
			Data.SetBlockTypeAtIndex( index, blockType );
		}

		protected override void OnDestroy()
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

		private void BuildMeshAndCollision()
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

		private void AddQuad( ChunkSlice slice, int x, int y, int z, int width, int height, int widthAxis, int heightAxis, int face, byte blockType, int brightness )
		{
			byte textureId = (byte)(blockType - 1);
			byte normal = (byte)face;
			uint faceData = (uint)((textureId & 31) << 18 | brightness | (normal & 7) << 27);
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

		BlockFace GetBlockFace( IntVector3 position, int side )
		{
			var p = Offset + position;
			var blockEmpty = Map.IsBlockEmpty( p );
			var blockType = blockEmpty ? (byte)0 : IsServer ? (byte)1 : Map.GetBlockTypeAtPosition( p );

			var face = new BlockFace
			{
				Side = (byte)side,
				Culled = blockType == 0,
				Type = blockType,
			};

			if ( !face.Culled && !Map.IsAdjacentBlockEmpty( p, side ) )
			{
				face.Culled = true;
			}

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
			int vertexOffset = 0;
			int axis = BlockDirectionAxis[direction];
			int sliceIndex = GetSliceIndex( position[axis], direction );
			var slice = Slices[sliceIndex];

			if ( slice.IsDirty ) return;

			slice.IsDirty = true;
			slice.Vertices.Clear();
			slice.CollisionVertices.Clear();
			slice.CollisionIndices.Clear();

			BlockFace faceA;
			BlockFace faceB;

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

					for ( faceWidth = 1; i + faceWidth < ChunkSize && !BlockFaceMask[n + faceWidth].Culled && BlockFaceMask[n + faceWidth].Equals( BlockFaceMask[n] ); faceWidth++ ) ;

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

					if ( !BlockFaceMask[n].Culled )
					{
						blockPosition[uAxis] = i;
						blockPosition[vAxis] = j;

						var brightness = (15 & 15) << 23;

						AddQuad( slice,
							blockPosition.x, blockPosition.y, blockPosition.z,
							faceWidth, faceHeight, uAxis, vAxis,
							BlockFaceMask[n].Side, BlockFaceMask[n].Type, brightness );

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

		private void UpdateBlockSlices()
		{
			IntVector3 blockPosition;
			IntVector3 blockOffset;

			BlockFace faceA;
			BlockFace faceB;

			for ( int faceSide = 0; faceSide < 6; faceSide++ )
			{
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

							for ( faceWidth = 1; i + faceWidth < ChunkSize && !BlockFaceMask[n + faceWidth].Culled && BlockFaceMask[n + faceWidth].Equals( BlockFaceMask[n] ); faceWidth++ ) ;

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

							if ( !BlockFaceMask[n].Culled )
							{
								blockPosition[uAxis] = i;
								blockPosition[vAxis] = j;

								var brightness = (15 & 15) << 23;

								AddQuad( slice,
									blockPosition.x, blockPosition.y, blockPosition.z,
									faceWidth, faceHeight, uAxis, vAxis,
									BlockFaceMask[n].Side, BlockFaceMask[n].Type, brightness );
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
