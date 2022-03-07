using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WhiteTorchBlock : BaseTorchBlock
	{
		public override string FriendlyName => "White Torch";
		public override IntVector3 LightLevel => new IntVector3( 13, 13, 13 );
	}
}
