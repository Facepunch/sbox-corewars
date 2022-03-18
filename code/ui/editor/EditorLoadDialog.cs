using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorLoadDialog : Panel
	{
		private static EditorLoadDialog Current { get; set; }

		public AutoCompleteInput Input { get; set; }
		public AutoCompleteList Suggestions { get; set; }
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

			FileSystem.Data.CreateDirectory( "worlds" );

			var files = FileSystem.Data.FindFile( "worlds", "*.voxels" );

			foreach ( var file in files )
			{
				var item = Items.AddChild<EditorLoadDialogItem>( "item" );
				item.FileName = file.Replace( ".voxels", "" );
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

			Input.SetAutoCompleteList( Suggestions );
			Input.AutoCompleteHandler = DoAutoComplete;

			if ( !string.IsNullOrEmpty( state.CurrentFileName ) )
			{
				Input.Text = state.CurrentFileName.Replace( "worlds/", "" ).Replace( ".voxels", "" );
				Input.CaretPosition = Input.TextLength;
			}

			Input.Focus();
			Input.AddEventListener( "onsubmit", () => HandleSubmit() );
			Input.AddEventListener( "onblur", () => HandleClose() );

			PopulateItems();

			base.PostTemplateApplied();
		}

		private string[] DoAutoComplete( string arg )
		{
			if ( string.IsNullOrEmpty( arg ) ) return null;

			FileSystem.Data.CreateDirectory( "worlds" );

			var files = FileSystem.Data.FindFile( "worlds", "*.voxels" );

			return files
				.Select( f => f.Replace( ".voxels", "" ) )
				.Where( f => f != arg && f.StartsWith( arg ) ).ToArray();
		}
	}
}
