using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class StealthBuff : BaseBuff
	{
		public override string Icon => "textures/items/brew_stealth.png";

		private Particles Effect { get; set; }

		public override void OnActivated( Player player )
		{
			if ( IsServer )
			{
				player.EnableDrawing = false;
			}

			if ( IsClient && player.IsLocalPawn )
			{
				Effect?.Destroy();
				Effect = Particles.Create( "particles/player/cloaked.vpcf" );
			}

			base.OnActivated( player );
		}

		public override void OnExpired( Player player )
		{
			if ( IsServer )
			{
				player.EnableDrawing = true;
			}

			Effect?.Destroy();

			base.OnExpired( player );
		}
	}
}
