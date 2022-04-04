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
	public partial class ToolSelectorMenu : SimpleRadialMenu
	{
		public override InputButton Button => InputButton.Score;

		public override void Populate()
		{
			var available = Library.GetAttributes<EditorToolAttribute>();

			foreach ( var attribute in available )
			{
				AddTool( attribute );
			}

			AddAction( "Block List", "View available blocks", "textures/ui/blocklist.png", () => EditorBlockList.Open() );

			if ( Local.Client.IsListenServerHost )
			{
				AddAction( "Save World", "Save world to disk", "textures/ui/save.png", () => EditorSaveDialog.Open() );
				AddAction( "Load World", "Load world from disk", "textures/ui/load.png", () => EditorLoadDialog.Open() );
			}

			base.Populate();
		}
	}
}
