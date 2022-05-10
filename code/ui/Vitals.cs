using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Vitals : Panel
	{
		public Panel HealthBar { get; set; }
		public Panel StaminaBar { get; set; }

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
			{
				HealthBar.Style.Width = Length.Fraction( player.Health / 100f );
				HealthBar.SetClass( "health-low", player.Health <= 15f );

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

		private bool IsHidden()
		{
			return IDialog.IsActive() || !Game.IsState<GameState>();
		}
	}
}
