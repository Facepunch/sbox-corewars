using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorBlockList : Panel
	{
		public static EditorBlockList Current { get; private set; }

		public Panel Items { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorBlockList();
			Current.PopulateItems();

			Game.Hud.AddChild( Current );
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
				if ( !block.HasTexture ) continue;
				var item = Items.AddChild<EditorBlockItem>();
				item.SetBlockId( block.BlockId );
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
