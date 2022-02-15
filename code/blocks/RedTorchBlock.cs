using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class RedTorchBlock : BaseTorchBlock
	{
		public override string FriendlyName => "Red Torch";
		public override IntVector3 LightLevel => new IntVector3( 14, 10, 10 );
	}
}
