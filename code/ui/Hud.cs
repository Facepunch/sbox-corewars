﻿using Facepunch.CoreWars.Voxel;
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

			var chunksLoaded = Map.Current.Chunks.Count( kv => kv.Value.Initialized );
			return $"{chunksLoaded} Chunks Loaded";
		}

		private bool HasWorldLoaded()
		{
			if ( Map.Current == null ) return false;
			return !Map.Current.Chunks.Any( kv => !kv.Value.Initialized );
		}
	}
}
