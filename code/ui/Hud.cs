using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Hud : RootPanel
	{
		public static Hud Current { get; private set; }
		public Panel Crosshair { get; set; }

		private Team Team { get; set; }

		public static void SetTeam( Team team )
		{
			if ( Current == null || Current.Team == team )
				return;

			Current.RemoveClass( Current.Team.GetHudClass() );
			Current.AddClass( team.GetHudClass() );

			Current.Team = team;
		}

		[ConCmd.Server]
		public static void TestToastCmd( string text )
		{
			Toast( To.Everyone, text, "textures/items/crystal.png" );
		}

		[ClientRpc]
		public static void AddKillFeed( Player attacker, Player victim, Entity weapon )
		{
			ToastList.Instance.AddKillFeed( attacker, victim, weapon );
		}

		[ClientRpc]
		public static void AddKillFeed( Player victim )
		{
			ToastList.Instance.AddKillFeed( victim );
		}

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

		public Hud()
		{
			AddChild<ChatBox>();
			AddChild<ToastList>();
			AddChild<VoiceList>();

			AddClass( Team.None.GetHudClass() );

			Current = this;
		}

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
			{
				SetTeam( player.Team );
			}

			base.Tick();
		}
	}
}
