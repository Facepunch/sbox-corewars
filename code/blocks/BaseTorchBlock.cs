using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public class BaseTorchBlock : BlockType
	{
		public override bool IsTranslucent => true;
		public override string ClientEntity => "cw_torch";
		public override bool HasTexture => false;
		public override bool IsPassable => true;

		public override BlockData CreateDataInstance() => new TorchBlockData();

		public override void OnBlockAdded( Chunk chunk, int x, int y, int z, int direction )
		{
			var data = VoxelWorld.GetOrCreateData<TorchBlockData>( new IntVector3( x, y, z ) );

			data.Direction = (BlockFace)direction;
			data.IsDirty = true;

			base.OnBlockAdded( chunk, x, y, z, direction );
		}
	}
}
