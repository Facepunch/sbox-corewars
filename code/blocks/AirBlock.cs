using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class AirBlock : BlockType
	{
		public override bool IsTranslucent => true;
		public override byte TextureId => 0;
		public override byte BlockId => 0;
	}
}
