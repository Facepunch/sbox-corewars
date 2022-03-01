using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class EditorHud : RootPanel
	{
		public EditorHud()
		{
			AddChild<ChatBox>();
		}
	}
}
