using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class LoadingScreen : Panel
	{
		public string ChunksLoaded => GetChunksLoaded();
		
		private bool DidWorldLoad { get; set; } = false;

		public override void Tick()
		{
			SetClass( "hidden", HasWorldLoaded() );

			base.Tick();
		}

		private string GetChunksLoaded()
		{
			if ( !VoxelWorld.Current.IsValid() ) return "Generating World...";

			var viewer = Local.Client.GetChunkViewer();
			if ( !viewer.IsValid() ) return "Initializing Player...";

			if ( !viewer.HasLoadedMinimumChunks() )
			{
				var minimumChunks = VoxelWorld.Current.MinimumLoadedChunks;
				var loadedChunks = viewer.LoadedChunks.Count;

				return $"Loading Minimum Chunks {loadedChunks}/{minimumChunks}";
			}

			if ( !viewer.IsCurrentChunkReady )
				return "Loading Spawn Chunk...";

			if ( Local.Client.Pawn is Player player )
			{
				if ( player.LifeState == LifeState.Dead )
					return $"Spawning Player...";
			}

			return "...";
		}

		private bool HasWorldLoaded()
		{
			var viewer = Local.Client.GetChunkViewer();
			if ( !viewer.IsValid() ) return false;

			if ( VoxelWorld.Current == null || !VoxelWorld.Current.Initialized )
				return false;

			if ( viewer.TimeSinceLastReset < 1f )
				return false;

			return viewer.HasLoadedMinimumChunks();
		}
	}
}
