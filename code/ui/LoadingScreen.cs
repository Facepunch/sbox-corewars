using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class LoadingScreen : Panel
	{
		public Panel Spinner { get; set; }

		public override void Tick()
		{
			SetClass( "hidden", HasWorldLoaded() );

			var transform = new PanelTransform();
			transform.AddRotation( 0f, 0f, MathF.Sin( Time.Now ) * 180f );
			Spinner.Style.Transform = transform;

			base.Tick();
		}

		private bool HasWorldLoaded()
		{
			var viewer = Local.Client.GetChunkViewer();
			if ( !viewer.IsValid() ) return false;

			if ( VoxelWorld.Current == null || !VoxelWorld.Current.Initialized )
				return false;

			if ( viewer.TimeSinceLastReset < 1f )
				return false;

			return viewer.HasLoadedMinimumChunks() && viewer.IsCurrentChunkReady;
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
