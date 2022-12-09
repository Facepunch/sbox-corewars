using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class SpeedBuff : BaseBuff
	{
		public override string Icon => "textures/items/brew_speed.png";

		public override void OnActivated( CoreWarsPlayer player )
		{
			if ( IsServer )
			{
				player.AddModifier( StatModifier.Speed, 0.2f );
			}

			base.OnActivated( player );
		}

		public override void OnExpired( CoreWarsPlayer player )
		{
			if ( IsServer )
			{
				player.TakeModifier( StatModifier.Speed, 0.2f );
			}

			base.OnExpired( player );
		}
	}
}
