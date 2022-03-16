using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class EditorCamera : CameraMode
	{
		private Vector3 LastPosition { get; set; }

		public float ZoomOut { get; set; } = 0f;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePosition;
			Rotation = pawn.EyeRotation;
			LastPosition = Position;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var targetPosition = pawn.EyePosition;

			if ( targetPosition.Distance( LastPosition ) < 300f )
				Position = Vector3.Lerp( targetPosition.WithZ( LastPosition.z ), targetPosition, 20f * Time.Delta );
			else
				Position = targetPosition;

			Viewer = pawn;
			Rotation = pawn.EyeRotation;
			LastPosition = Position;
			FieldOfView = 80f + (20f * ZoomOut);
		}
	}
}
