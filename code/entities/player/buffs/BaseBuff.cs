using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class BaseBuff : BaseNetworkable
	{
		[Net] public RealTimeUntil TimeUntilExpired { get; set; }

		public virtual Color Color => UI.ColorPalette.Brews;
		public virtual float Duration => 30f;
		public virtual string Icon => "textures/ui/unknown.png";

		public virtual void OnActivated( CoreWarsPlayer player )
		{

		}

		public virtual void OnExpired( CoreWarsPlayer player )
		{
			if ( Game.IsClient && player.IsLocalPawn )
			{
				Sound.FromScreen( "buff.expired" );
			}
		}
	}
}
