using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Hud : RootPanel
	{
		public Panel LoadingScreen { get; private set; }
		public string ChunksLoaded => GetChunksLoaded();
		
		private bool DidWorldLoad { get; set; } = false;

		public Hud()
		{
			AddChild<ChatBox>();
		}

		public override void Tick()
		{
			if ( !Map.Current.IsValid() ) return;

			var viewer = Local.Client.GetChunkViewer();
			if ( !viewer.IsValid() ) return;

			if ( viewer.HasLoadedMinimumChunks() )
				DidWorldLoad = true;

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			LoadingScreen.BindClass( "hidden", HasWorldLoaded );

			base.PostTemplateApplied();
		}

		private string GetChunksLoaded()
		{
			if ( !Map.Current.IsValid() ) return "Generating Map...";

			var viewer = Local.Client.GetChunkViewer();
			if ( !viewer.IsValid() ) return "Initializing Player...";

			if ( !viewer.HasLoadedMinimumChunks() )
			{
				var minimumChunks = Map.Current.MinimumLoadedChunks;
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

			return string.Empty;
		}

		private bool HasWorldLoaded()
		{
			if ( Map.Current == null || !Map.Current.Initialized )
				return false;

			return DidWorldLoad;
		}
	}
}
