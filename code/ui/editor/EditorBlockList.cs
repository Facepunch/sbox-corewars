using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorBlockList : Panel
	{
		public static EditorBlockList Current { get; private set; }

		[ConVar.Client( "cw_show_all_blocks" ), Change( nameof( OnShowAllBlocksChanged ) )]
		public static bool ShowAllBlocks { get; set; }

		private static void OnShowAllBlocksChanged( bool oldValue, bool newValue )
		{
			Current?.PopulateItems();
		}

		public Panel Items { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorBlockList();
			Current.PopulateItems();
			Local.Hud.AddChild( Current );
		}

		public EditorBlockList()
		{
			AcceptsFocus = true;
			Focus();
		}

		public void PopulateItems()
		{
			Items.DeleteChildren();

			var world = VoxelWorld.Current;
			var blocks = world.BlockData.Values;

			foreach ( var block in blocks )
			{
				if ( block.ShowInEditor || ShowAllBlocks )
				{
					var item = Items.AddChild<EditorBlockItem>();
					item.SetBlockId( block.BlockId );
				}
			}
		}

		protected override void OnBlur( PanelEvent e )
		{
			Delete();
			base.OnBlur( e );
		}

		public override void OnButtonTyped( string button, KeyModifiers km )
		{
			if ( button == "escape" )
			{
				Blur();
			}

			base.OnButtonTyped( button, km );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
