using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars.UI;

public partial class RespawnScreen
{
	[ClientRpc]
	public static void Show( float respawnTime, Entity attacker, Entity weapon = null )
	{
		Instance.SetClass( "hidden", false );

		if ( attacker is CoreWarsPlayer player )
			Instance.KillerInfo.Update( player );
		else if ( !attacker.IsValid() || attacker.IsWorld )
			Instance.KillerInfo.Update( "Unknown" );
		else if ( attacker is IKillFeedInfo info )
			Instance.KillerInfo.Update( info );
		else
			Instance.KillerInfo.Update( attacker.Name );

		Instance.KillerInfo.SetWeapon( weapon );
		Instance.RespawnTime = respawnTime;
	}

	[ClientRpc]
	public static void Hide()
	{
		Instance.SetClass( "hidden", true );
	}
}
