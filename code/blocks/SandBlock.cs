using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class SandBlock : BlockType
	{
		public override string FriendlyName => "Sand";
		public override byte TextureId => 3;
		public override byte BlockId => 2;
	}
}
