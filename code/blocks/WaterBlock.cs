using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WaterBlock : BlockType
	{
		public override string DefaultTexture => "stone";
		public override string FriendlyName => "Water";
		public override bool IsPassable => true;
	}
}
