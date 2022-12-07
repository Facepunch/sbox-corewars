using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class EditorCamera
	{
		private Vector3 LastPosition { get; set; }

		public float ZoomOut { get; set; } = 0f;

		public void Update()
		{
			var pawn = Local.Pawn as EditorPlayer;
			if ( pawn == null ) return;

			var targetPosition = pawn.EyePosition;

			if ( targetPosition.Distance( LastPosition ) < 300f )
				Camera.Position = Vector3.Lerp( targetPosition.WithZ( LastPosition.z ), targetPosition, 20f * Time.Delta );
			else
				Camera.Position = targetPosition;

			Camera.FirstPersonViewer = pawn;
			Camera.Rotation = pawn.EyeRotation;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 80f + (20f * ZoomOut) );

			LastPosition = Camera.Position;
		}
	}
}
