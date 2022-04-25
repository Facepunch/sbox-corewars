using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class TemperedGlassBlock : BlockType
	{
		public override string DefaultTexture => "tempered_glass_01_color";
		public override string FriendlyName => "Tempered Glass";
		public override bool IsTranslucent => true;
	}
}
