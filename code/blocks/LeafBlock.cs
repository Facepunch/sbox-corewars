using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class LeafBlock : BlockType
	{
		public override string DefaultTexture => "grass_01";
		public override string FriendlyName => "Leaf";
		public override bool IsTranslucent => true;
	}
}
