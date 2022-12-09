using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class BaseBuff : BaseNetworkable
	{
		[Net] public RealTimeUntil TimeUntilExpired { get; set; }

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public virtual Color Color => UI.ColorPalette.Brews;
		public virtual float Duration => 30f;
		public virtual string Icon => "textures/ui/unknown.png";

		public virtual void OnActivated( Player player )
		{

		}

		public virtual void OnExpired( Player player )
		{
			if ( IsClient && player.IsLocalPawn )
			{
				Sound.FromScreen( "buff.expired" );
			}
		}
	}
}
