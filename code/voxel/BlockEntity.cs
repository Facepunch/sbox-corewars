using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	[Hammer.Skip]
	public class BlockEntity : ModelEntity
	{
		public IntVector3 LocalBlockPosition { get; set; }
		public IntVector3 BlockPosition { get; set; }
		public BlockType BlockType { get; set; }
		public Chunk Chunk { get; set; }
		public Map Map { get; set; }

		public void CenterOnBlock( bool centerHorizontally = true, bool centerVertically = true )
		{
			var voxelSize = Chunk.VoxelSize;
			var centerBounds = Vector3.Zero;

			if ( centerHorizontally )
			{
				centerBounds.x = voxelSize;
				centerBounds.y = voxelSize;
			}

			if ( centerVertically )
			{
				centerBounds.z = voxelSize;
			}

			Position = Map.ToSourcePosition( BlockPosition ) + centerBounds * 0.5f;
		}

		public virtual void Initialize()
		{

		}
	}
}
