using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public partial class CrossbowBoltProjectile : BulletDropProjectile
	{
		public override void CreateEffects()
        {
			base.CreateEffects();

			if ( Owner is CoreWarsPlayer player )
			{
				Trail?.SetPosition( 6, player.Team.GetColor() * 255f );
			}
		}
	}
}
