using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.CoreWars.UI
{
	public class KillFeedItem : Panel
	{
		public Label Attacker { get; set; }
		public Label Victim { get; set; }
		public Label Reason { get; set; }

		private float EndTime { get; set; }

		public KillFeedItem()
		{
			Attacker = Add.Label( "", "attacker" );
			Reason = Add.Label( "", "reason" );
			Victim = Add.Label( "", "victim" );
		}

		public void Update( Player victim, bool isFallDamage )
		{
			Attacker.SetClass( "hidden", true );
			AddClass( "is-suicide" );

			Victim.Text = victim.Client.Name;
			Victim.AddClass( victim.Team.GetHudClass() );

			if ( isFallDamage )
			{
				Reason.Text = "fell into the void";
			}
			else
			{
				var reasons = new string[] { "decided to end it all" };
				Reason.Text = Rand.FromArray( reasons );
			}

			EndTime = Time.Now + 3f;
		}

		public void Update( Player attacker, Player victim, Entity weapon )
		{
			Attacker.Text = attacker.Client.Name;
			Attacker.AddClass( attacker.Team.GetHudClass() );

			Victim.Text = victim.Client.Name;
			Victim.AddClass( victim.Team.GetHudClass() );

			Reason.Text = "killed";

			if ( weapon is IKillFeedInfo info )
			{
				if ( info.KillFeedReasons.Length > 0 )
				{
					Reason.Text = Rand.FromArray( info.KillFeedReasons );
				}
			}

			EndTime = Time.Now + 3f;
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
				Delete();
		}
	}

	public class ToastItem : Panel
	{
		public Label Text { get; set; }
		public Image Icon { get; set; }

		private float EndTime { get; set; }

		public ToastItem()
		{
			Icon = Add.Image( "", "icon" );
			Text = Add.Label( "", "text" );
		}

		public void Update( string text, Texture icon = null )
		{
			Icon.Texture = icon;
			Text.Text = text;

			Icon.SetClass( "hidden", icon == null );

			EndTime = Time.Now + 6f;
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
				Delete();
		}
	}

    [StyleSheet( "/ui/ToastList.scss" )]
	public class ToastList : Panel
	{
		public static ToastList Instance { get; private set; }

		public Panel KillFeedContainer { get; set; }
		public Panel ToastsContainer { get; set; }

		public ToastList()
		{
			KillFeedContainer = Add.Panel( "killfeed" );
			ToastsContainer = Add.Panel( "toasts" );
			Instance = this;
		}

		public void AddKillFeed( Player attacker, Player victim, Entity weapon )
		{
			var item = KillFeedContainer.AddChild<KillFeedItem>();
			item.Update( attacker, victim, weapon );
		}

		public void AddKillFeed( Player victim, bool isFallDamage )
		{
			var item = KillFeedContainer.AddChild<KillFeedItem>();
			item.Update( victim, isFallDamage );
		}

		public void AddItem( string text, Texture icon = null )
		{
			var item = ToastsContainer.AddChild<ToastItem>();
			item.Update( text, icon );
		}

		public override void Tick()
		{
			ToastsContainer.SetClass( "is-editor", Game.Current.IsEditorMode );
			base.Tick();
		}
	}
}
