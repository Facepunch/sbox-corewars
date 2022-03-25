using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EntityToolMenu : SimpleRadialMenu
	{
		public override InputButton Button => InputButton.Reload;

		public override void Populate()
		{
			AddAction( "Test", "Test", "textures/ui/blocklist.png", () => Log.Info( "Test" ) );

			base.Populate();
		}
	}
}
