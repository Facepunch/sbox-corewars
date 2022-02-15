using Facepunch.CoreWars.Voxel;
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

		public Hud()
		{
			AddChild<ChatBox>();
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

			return string.Empty;
		}

		private bool HasWorldLoaded()
		{
			if ( Map.Current == null || !Map.Current.Initialized )
				return false;

			var viewer = Local.Client.GetChunkViewer();
			if ( !viewer.IsValid() ) return false;

			return viewer.HasLoadedMinimumChunks();
		}
	}
}
