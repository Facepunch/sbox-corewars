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
			return string.Empty;
		}

		private bool HasWorldLoaded()
		{
			if ( Map.Current == null || !Map.Current.Initialized )
				return false;

			return true;
		}
	}
}
