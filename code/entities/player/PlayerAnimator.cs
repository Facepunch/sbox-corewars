using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public partial class PlayerAnimator : StandardPlayerAnimator
	{
		private float Skid { get; set; }

		public override void Simulate()
		{
			base.Simulate();

			if ( Pawn is not Player player )
				return;

			if ( Velocity.Length > 90f )
			{
				if ( player.InputDirection.x == 0f && player.InputDirection.y == 0f )
					Skid = Skid.LerpTo( 1f, Time.Delta * 5f );
				else
					Skid = Skid.LerpTo( 0f, Time.Delta * 5f );
			}
			else
			{
				Skid = Skid.LerpTo( 0f, Time.Delta * 5f );
			}

			SetAnimParameter( "skid", Skid );
		}
	}
}
