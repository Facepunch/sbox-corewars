using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WaterBlock : BlockType
	{
		public override string DefaultTexture => "water";
		public override string FriendlyName => "Water";
		public override bool IsTranslucent => false;
		public override bool IsPassable => true;
		public override bool IsLiquid => true;
	}
}
