using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class CobbleBlock : BlockType
	{
		public override string FriendlyName => "Cobble";
		public override byte TextureId => 8;
		public override byte BlockId => 6;
	}
}
