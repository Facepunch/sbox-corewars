using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkSlice
	{
		public ChunkSlice()
		{
			Body = PhysicsWorld.WorldBody;
		}

		public bool IsDirty = false;
		public List<BlockVertex> TranslucentVertices = new();
		public List<BlockVertex> OpaqueVertices = new();
		public List<Vector3> CollisionVertices = new();
		public List<int> CollisionIndices = new();
		public PhysicsBody Body;
		public PhysicsShape Shape;
	}
}
