using Sandbox.UI;
using Sandbox;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public class Crosshair : Panel
	{
		public static Crosshair Current { get; private set; }

		private TimeSince LastHitTime { get; set; }

		public Crosshair()
		{
			Current = this;
		}

		public void Hit( int hitboxGroup )
		{
			if ( hitboxGroup == 1 )
				PlaySound( "hitmarker.headshot" );
			else
				PlaySound( "hitmarker.hit" );

			SetClass( "hit-marker", true );
			LastHitTime = 0f;
		}

		public override void Tick()
		{
			if ( LastHitTime > 0.1f )
			{
				SetClass( "hit-marker", false );
			}

			base.Tick();
		}
	}
}
