using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars
{
	[UseTemplate] 
	public partial class RespawnScreen : Panel
	{
		public static RespawnScreen Instance { get; private set; }

		public RespawnKillerInfo KillerInfo { get; private set; }
		public Label RespawnTimeLabel { get; private set; }
		public Label KilledByLabel { get; private set; }

		public RealTimeUntil RespawnTime { get; private set; }

		public string RespawnTimeLeft => GetRespawnTimeLeft();

		public RespawnScreen()
		{
			SetClass( "hidden", true );
			Instance = this;
		}

		[ClientRpc]
		public static void Show( float respawnTime, Entity attacker, Entity weapon = null )
		{
			Instance.SetClass( "hidden", false );

			if ( attacker is Player player )
				Instance.KillerInfo.Update( player );
			else if ( attacker.IsWorld )
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

		private string GetRespawnTimeLeft()
		{
			var timeLeftWithMax = Math.Max( RespawnTime.Relative.CeilToInt(), 0 );
			return $"{TimeSpan.FromSeconds( timeLeftWithMax ).ToString( @"mm\:ss" )}";
		}
	}
}
