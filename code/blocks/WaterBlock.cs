using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WaterBlock : LiquidBlock
	{
		public override string DefaultTexture => "water";
		public override string FriendlyName => "Water";
		public override bool AttenuatesSunLight => true;
		public override bool UseTransparency => true;
		public override bool IsTranslucent => true;
	}
}
