using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WindowBlock : BlockType
	{
		public override string DefaultTexture => "window";
		public override string FriendlyName => "Window";
		public override bool IsTranslucent => true;
	}
}
