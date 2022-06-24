using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class EditorHud : RootPanel
	{
		public Panel PopupPanel { get; set; }

		public static void ToastAll( string text, string icon = "" )
		{
			Toast( To.Everyone, text, icon );
		}

		public static void Toast( Player player, string text, string icon = "" )
		{
			Toast( To.Single( player ), text, icon );
		}

		[ClientRpc]
		public static void Toast( string text, string icon = "" )
		{
			ToastList.Instance.AddItem( text, Texture.Load( FileSystem.Mounted, icon ) );
		}

		public EditorHud()
		{
			AddChild<ChatBox>();
			AddChild<ToastList>();
			AddChild<VoiceList>();
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
