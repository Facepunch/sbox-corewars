using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class DamageBuff : BaseBuff
	{
		public override string Icon => "textures/items/brew_damage.png";

		public override void OnActivated( CoreWarsPlayer player )
		{
			if ( Game.IsServer )
			{
				player.AddModifier( StatModifier.Damage, 0.15f );
			}

			base.OnActivated( player );
		}

		public override void OnExpired( CoreWarsPlayer player )
		{
			if ( Game.IsServer )
			{
				player.TakeModifier( StatModifier.Damage, 0.15f );
			}

			base.OnExpired( player );
		}
	}
}
