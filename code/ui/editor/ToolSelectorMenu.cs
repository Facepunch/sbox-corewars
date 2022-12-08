using Facepunch.CoreWars.Editor;

using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class ToolSelectorMenu : SimpleRadialMenu
	{
		public override InputButton Button => InputButton.Score;

		public override void Populate()
		{
			var available = TypeLibrary.GetDescriptions<EditorTool>().ToList();

			available.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			foreach ( var type in available )
			{
				if ( !type.IsAbstract )
				{
					AddTool( type );
				}
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
