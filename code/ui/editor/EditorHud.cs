using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class EditorHud : RootPanel
	{
		public Panel PopupPanel { get; set; }

		public EditorHud()
		{
			AddChild<ChatBox>();
		}

		public override Panel FindPopupPanel()
		{
			return PopupPanel;
		}

		protected override void PostTemplateApplied()
		{
			PopupPanel.BindClass( "open", () => PopupPanel.ChildrenCount > 0 );

			base.PostTemplateApplied();
		}
	}
}
