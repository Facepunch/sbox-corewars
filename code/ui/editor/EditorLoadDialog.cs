using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorLoadDialog : Panel
	{
		private static EditorLoadDialog Current { get; set; }

		public TextEntry Input { get; set; }
		public Panel Items { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorLoadDialog();

			Game.Hud.AddChild( Current );
		}

		public EditorLoadDialog()
		{
			PopulateItems();
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			base.Tick();
		}

		protected void PopulateItems()
		{
			Items.DeleteChildren();

			var files = FileSystem.Data.FindFile( "", "*" );

			foreach ( var file in files )
			{
				var item = Items.AddChild<EditorLoadDialogItem>( "item" );
				item.FileName = file;
				item.OnSelect = () => OpenFile( item.FileName );
			}
		}

		protected virtual void OpenFile( string fileName )
		{
			Game.LoadEditorMapCmd( fileName );
			Delete();
		}

		protected virtual void HandleSubmit()
		{
			OpenFile( Input.Text );
		}

		protected virtual void HandleClose()
		{
			Delete();
		}

		protected override void PostTemplateApplied()
		{
			var state = Game.GetStateAs<EditorState>();

			if ( !string.IsNullOrEmpty( state.CurrentFileName ) )
			{
				Input.Text = state.CurrentFileName;
				Input.CaretPosition = Input.TextLength;
			}

			Input.Focus();
			Input.AddEventListener( "onsubmit", () => HandleSubmit() );
			Input.AddEventListener( "onblur", () => HandleClose() );

			PopulateItems();

			base.PostTemplateApplied();
		}
	}
}
