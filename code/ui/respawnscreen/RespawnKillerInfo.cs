using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public class RespawnKillerInfo : Panel
	{
		public Image KillerAvatar { get; private set; }
		public Label KillerName { get; private set; }
		public Label WeaponName { get; private set; }

		private Team CurrentTeam { get; set; }

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void Update( Player player )
		{
			KillerAvatar.SetTexture( $"avatar:{player.Client.SteamId}" );
			KillerName.Style.FontColor = player.Team.GetColor().Darken( 0.4f );
			KillerName.Text = player.Client.Name;
			SetTeam( player.Team );
		}

		public void Update( IKillFeedInfo killer )
		{
			KillerAvatar.Texture = Texture.Load( FileSystem.Mounted, killer.KillFeedIcon );
			KillerName.Style.FontColor = killer.KillFeedTeam.GetColor().Darken( 0.4f );
			KillerName.Text = killer.KillFeedName;
			SetTeam( killer.KillFeedTeam );
		}

		public void Update( string killerName )
		{
			KillerAvatar.Texture = Texture.Load( FileSystem.Mounted, "textures/ui/skull.png" );
			KillerName.Style.FontColor = Team.None.GetColor().Darken( 0.4f );
			KillerName.Text = killerName;
			SetTeam( Team.None );
		}

		public void SetTeam( Team team )
		{
			SetClass( CurrentTeam.GetHudClass(), false );
			SetClass( team.GetHudClass(), true );
			CurrentTeam = team;
		}

		public void SetWeapon( Entity weapon )
		{
			if ( weapon.IsValid() && !weapon.IsWorld )
			{
				SetClass( "has-weapon", true );

				WeaponName.Style.FontColor = CurrentTeam.GetColor().Darken( 0.4f );

				if ( weapon is IKillFeedInfo feedInfo )
					WeaponName.Text = feedInfo.KillFeedName;
				else if ( weapon is Weapon typed && typed.Item.IsValid() )
					WeaponName.Text = typed.Item.Instance.Name;
				else
					WeaponName.Text = weapon.Name;
			}
			else
			{
				SetClass( "has-weapon", false );
			}
		}
	}
}
