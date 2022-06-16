using Facepunch.CoreWars.Utility;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public class BuffIcon : Panel
	{
		public BaseBuff Buff { get; private set; }
		public Panel Icon { get; private set; }
		public Panel Timer { get; private set; }

		public BuffIcon()
		{
			Icon = Add.Panel( "icon" );
			Timer = Add.Panel( "timer" );
		}

		public void SetBuff( BaseBuff buff )
		{
			Style.SetLinearGradientBackground( buff.Color, 0.5f, new Color( 0.2f ), 0.5f );
			Icon.Style.SetBackgroundImage( buff.Icon );
			Buff = buff;
		}

		public override void Tick()
		{
			Timer.Style.Height = Length.Fraction( 1f - ( Buff.TimeUntilExpired / Buff.Duration ) );
			base.Tick();
		}
	}

	[UseTemplate]
	public partial class Vitals : Panel
	{
		public static Vitals Current { get; private set; }

		public Panel HealthBar { get; set; }
		public Panel StaminaBar { get; set; }

		public Label HealthValue { get; set; }
		public Label StaminaValue { get; set; }

		public Panel BuffsContainer { get; set; }

		public List<BuffIcon> Icons { get; set; } = new();

		public static void AddBuff( BaseBuff buff )
		{
			Current?.InternalAddBuff( buff );
		}

		public static void RemoveBuff( BaseBuff buff )
		{
			Current?.InternalRemoveBuff( buff );
		}

		public Vitals()
		{
			Current = this;
		}

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
			{
				HealthBar.Style.Width = Length.Fraction( player.Health / 100f );
				HealthBar.SetClass( "health-low", player.Health <= 15f );

				HealthValue.Text = $"{player.Health.CeilToInt()}%";
				StaminaValue.Text = $"{player.Stamina.CeilToInt()}%";

				StaminaBar.Style.Width = Length.Fraction( player.Stamina / 100f );
				StaminaBar.SetClass( "stamina-low", player.IsOutOfBreath );
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			if ( Local.Pawn is not Player player )
				return;

			BindClass( "hidden", IsHidden );

			base.PostTemplateApplied();
		}

		private void InternalAddBuff( BaseBuff buff )
		{
			var icon = BuffsContainer.AddChild<BuffIcon>( "buff" );
			icon.SetBuff( buff );
			Icons.Add( icon );
		}

		private void InternalRemoveBuff( BaseBuff buff )
		{
			var icon = Icons.Find( i => i.Buff == buff );

			if ( icon != null )
			{
				Icons.Remove( icon );
				icon.Delete();
			}
		}

		private bool IsHidden()
		{
			if ( Local.Pawn.LifeState == LifeState.Dead )
				return true;

			if ( IDialog.IsActive() || !Game.IsState<GameState>() )
				return true;

			return false;
		}
	}
}
