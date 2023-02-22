using Sandbox;

namespace Facepunch.CoreWars
{
	public partial class FirstPersonCamera
	{
		public void Update()
		{
			if ( Game.LocalPawn is not CoreWarsPlayer player )
				return;

			Camera.Rotation = player.ViewAngles.ToRotation();
			Camera.Position = player.EyePosition;
			Camera.FieldOfView = Game.Preferences.FieldOfView;
			Camera.FirstPersonViewer = player;
			Camera.ZNear = 1f;
			Camera.ZFar = 5000f;

			ScreenShake.Apply();
		}
	}
}
