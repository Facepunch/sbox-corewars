using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class DamageBuff : BaseBuff
	{
		public override string Icon => "textures/items/brew_damage.png";

		public override void OnActivated( Player player )
		{
			if ( IsServer )
			{
				player.AddModifier( StatModifier.Damage, 0.15f );
			}

			base.OnActivated( player );
		}

		public override void OnExpired( Player player )
		{
			if ( IsServer )
			{
				player.TakeModifier( StatModifier.Damage, 0.15f );
			}

			base.OnExpired( player );
		}
	}
}
