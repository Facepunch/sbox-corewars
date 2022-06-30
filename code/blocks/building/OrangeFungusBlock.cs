using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class OrangeFungusBlock : BaseFungusBlock
	{
		public override string DefaultTexture => "fungus";
		public override Color TintColor => Team.Orange.GetColor();
	}
}

