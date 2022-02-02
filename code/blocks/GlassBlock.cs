using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GlassBlock : BlockType
	{
		public override string FriendlyName => "Glass";
		public override bool IsTranslucent => false;
		public override byte TextureId => 5;
		public override byte BlockId => 4;
		public override byte LightLevel => 14;
	}
}
