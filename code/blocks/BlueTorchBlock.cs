using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class BlueTorchBlock : BaseTorchBlock
	{
		public override string FriendlyName => "Blue Torch";
		public override IntVector3 LightLevel => new IntVector3( 0, 0, 12 );
	}
}
