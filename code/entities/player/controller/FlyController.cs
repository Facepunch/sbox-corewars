using Sandbox;

namespace Facepunch.CoreWars
{
	public partial class FlyController : BaseMoveController
	{
		[Net] public bool EnableCollisions { get; set; } = true;

		protected float EyeHeight { get; set; } = 72f;
		protected float BodyGirth { get; set; } = 32f;
		protected float BodyHeight { get; set; } = 72f;

		protected Vector3 TraceOffset { get; set; }
		protected Vector3 Mins { get; set; }
		protected Vector3 Maxs { get; set; }

		public override void Simulate()
		{
			base.Simulate();

			Player.EyeLocalPosition = Vector3.Up * Scale( EyeHeight );
			UpdateBBox();

			Player.EyeLocalPosition += TraceOffset;
			Player.EyeRotation = Player.ViewAngles.ToRotation();

			var vel = (Player.EyeRotation.Forward * Player.InputDirection.x) + (Player.EyeRotation.Left * Player.InputDirection.y);

			vel = vel.Normal * 2000;

			if ( Input.Down( InputButton.Run ) )
				vel *= 5.0f;

			if ( Input.Down( InputButton.Duck ) )
				vel *= 0.2f;

			Player.Velocity += vel * Time.Delta;

			if ( Player.Velocity.LengthSquared > 0.01f )
			{
				Move();
			}

			Player.Velocity = Player.Velocity.Approach( 0, Player.Velocity.Length * Time.Delta * 5.0f );

			if ( Input.Down( InputButton.Jump ) )
				Player.Velocity = Player.Velocity.Approach( 0, Player.Velocity.Length * Time.Delta * 5.0f );
		}

		private void Move()
		{
			if ( !EnableCollisions )
			{
				Player.Position += Player.Velocity * Time.Delta;
				return;
			}

			var mover = new MoveHelper( Player.Position, Player.Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Player );
			mover.TryMove( Time.Delta );

			Player.Position = mover.Position;
			Player.Velocity = mover.Velocity;
		}

		private BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );
			return new BBox( mins, maxs );
		}

		private float Scale( float speed )
		{
			return speed * Player.Scale;
		}

		private Vector3 Scale( Vector3 velocity )
		{
			return velocity * Player.Scale;
		}

		private void SetBBox( Vector3 mins, Vector3 maxs )
		{
			if ( Mins == mins && Maxs == maxs )
				return;

			Mins = mins;
			Maxs = maxs;
		}

		private void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;
			var mins = Scale( new Vector3( -girth, -girth, 0 ) );
			var maxs = Scale( new Vector3( +girth, +girth, BodyHeight ) );
			SetBBox( mins, maxs );
		}
	}
}
