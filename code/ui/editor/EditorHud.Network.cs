using Sandbox;

namespace Facepunch.CoreWars.Editor;

public partial class EditorHud
{
    [ClientRpc]
	public static void Toast( string text, string icon = "" )
	{
		UI.ToastList.Instance.AddItem( text, Texture.Load( FileSystem.Mounted, icon ) );
	}
}
