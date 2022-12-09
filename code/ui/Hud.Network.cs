using Sandbox;

namespace Facepunch.CoreWars.UI;

public partial class Hud
{
	[ClientRpc]
	public static void AddKillFeed( CoreWarsPlayer attacker, CoreWarsPlayer victim, Entity weapon )
	{
		ToastList.Instance.AddKillFeed( attacker, victim, weapon );
	}

	[ClientRpc]
	public static void AddKillFeed( CoreWarsPlayer victim, bool isFallDamage )
	{
		ToastList.Instance.AddKillFeed( victim, isFallDamage );
	}

	[ClientRpc]
	public static void Toast( string text, string icon = "" )
	{
		ToastList.Instance.AddItem( text, Texture.Load( FileSystem.Mounted, icon ) );
	}
}
