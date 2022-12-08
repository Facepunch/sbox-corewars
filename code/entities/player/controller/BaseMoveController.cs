using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public class BaseMoveController : BaseNetworkable
	{
		public Vector3 WishVelocity { get; protected set; }
		public BasePlayer Player { get; protected set; }

		protected HashSet<string> Events { get; set; } = new();
		protected HashSet<string> Tags { get; set; } = new();

		public void SetActivePlayer( BasePlayer player )
		{
			Player = player;
		}

		public bool HasEvent( string eventName )
		{
			if ( Events == null ) return false;
			return Events.Contains( eventName );
		}

		public bool HasTag( string tagName )
		{
			if ( Tags == null ) return false;
			return Tags.Contains( tagName );
		}

		public void AddEvent( string eventName )
		{
			if ( Events == null )
				Events = new HashSet<string>();

			if ( Events.Contains( eventName ) )
				return;

			Events.Add( eventName );
		}

		public void SetTag( string tagName )
		{
			Tags ??= new HashSet<string>();

			if ( Tags.Contains( tagName ) )
				return;

			Tags.Add( tagName );
		}

		public virtual void FrameSimulate()
		{
			Assert.NotNull( Player );
		}

		public virtual void Simulate()
		{
			Assert.NotNull( Player );

			Events?.Clear();
			Tags?.Clear();
		}
	}
}
