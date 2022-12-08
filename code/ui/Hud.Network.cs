using Sandbox;

namespace Facepunch.CoreWars.UI;

public partial class Hud
{
	[ClientRpc]
	public static void AddKillFeed( Player attacker, Player victim, Entity weapon, DamageFlags flags )
	{
		ToastList.Instance.AddKillFeed( attacker, victim, weapon, flags );
	}

	[ClientRpc]
	public static void AddKillFeed( Player victim, DamageFlags flags )
	{
		ToastList.Instance.AddKillFeed( victim, flags );
	}

	[ClientRpc]
	public static void Toast( string text, string icon = "" )
	{
		ToastList.Instance.AddItem( text, Texture.Load( FileSystem.Mounted, icon ) );
	}
}
