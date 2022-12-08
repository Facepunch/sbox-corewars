using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorSaveDialog : Panel
	{
		private static EditorSaveDialog Current { get; set; }

		public AutoCompleteInput Input { get; set; }
		public AutoCompleteList Suggestions { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorSaveDialog();

			Local.Hud.AddChild( Current );
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			base.Tick();
		}

		protected virtual void HandleSubmit()
		{
			Game.SaveEditorMapCmd( Input.Text );
		}

		protected virtual void HandleClose()
		{
			Delete();
		}

		protected override void PostTemplateApplied()
		{
			Input.AutoCompleteHandler = DoAutoComplete;
			Input.AutoCompleteList = Suggestions;

			var state = Game.GetStateAs<EditorState>();

			if ( !string.IsNullOrEmpty( state.CurrentFileName ) )
			{
				Input.Text = state.CurrentFileName.Replace( "worlds/", "" ).Replace( ".voxels", "" );
				Input.CaretPosition = Input.TextLength;
			}

			Input.Focus();
			Input.AddEventListener( "onsubmit", () => HandleSubmit() );
			Input.AddEventListener( "onblur", () => HandleClose() );

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
