using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GreenTorchBlock : BaseTorchBlock
	{
		public override string FriendlyName => "Green Torch";
		public override IntVector3 LightLevel => new IntVector3( 0, 6, 0 );
	}
}
