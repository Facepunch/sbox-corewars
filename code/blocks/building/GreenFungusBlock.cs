using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GreenFungusBlock : BaseFungusBlock
	{
		public override string DefaultTexture => "fungus";
		public override Color TintColor => Team.Green.GetColor();
	}
}

