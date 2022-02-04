using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class MetalgrateBlock : BlockType
	{
		public override string DefaultTexture => "metalgrate";
		public override string FriendlyName => "Metal Grate";
		public override bool IsTranslucent => true;
	}
}
