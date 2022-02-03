using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class LeafBlock : BlockType
	{
		public override string FriendlyName => "Leaf";
		public override bool IsTranslucent => true;
		public override byte TextureId => 9;
		public override byte BlockId => 7;
	}
}
