using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class LeafBlock : BlockType
	{
		public override string DefaultTexture => "leaf";
		public override string FriendlyName => "Leaf";
		public override bool IsTranslucent => true;
	}
}
