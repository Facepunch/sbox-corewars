using Sandbox;

namespace Facepunch.CoreWars
{
	public class FlyController : BasePlayerController
	{
		public float EyeHeight { get; set; } = 72f;
		public float BodyGirth { get; set; } = 32f;
		public float BodyHeight { get; set; } = 72f;
		public bool EnableCollisions { get; set; } = true;

		protected Vector3 Mins { get; set; }
		protected Vector3 Maxs { get; set; }

		public override BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );
			return new BBox( mins, maxs );
		}

		public override void Simulate()
		{
			EyeLocalPosition = Vector3.Up * Scale( EyeHeight );
			UpdateBBox();

			EyeLocalPosition += TraceOffset;
			EyeRotation = Input.Rotation;

			var vel = (Input.Rotation.Forward * Input.Forward) + (Input.Rotation.Left * Input.Left);

			vel = vel.Normal * 2000;

			if ( Input.Down( InputButton.Run ) )
				vel *= 5.0f;

			if ( Input.Down( InputButton.Duck ) )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Move();
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );

			if ( Input.Down( InputButton.Jump ) )
				Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );
		}

		private void Move()
		{
			if ( !EnableCollisions )
			{
				Position += Velocity * Time.Delta;
				return;
			}

			var mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Pawn );
			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		private float Scale( float speed )
		{
			return speed * Pawn.Scale;
		}

		private Vector3 Scale( Vector3 velocity )
		{
			return velocity * Pawn.Scale;
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
