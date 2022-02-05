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
	}
}
