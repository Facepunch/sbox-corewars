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

		public override BlockState CreateState() => new TorchState();

		public override void OnBlockAdded( Chunk chunk, IntVector3 position, int direction )
		{
			var state = World.GetOrCreateState<TorchState>( position );

			state.Direction = (BlockFace)direction;
			state.IsDirty = true;

			base.OnBlockAdded( chunk, position, direction );
		}
	}
}
