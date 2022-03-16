using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorSaveDialog : Panel
	{
		private static EditorSaveDialog Current { get; set; }

		public TextEntry Input { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorSaveDialog();

			Game.Hud.AddChild( Current );
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
			var state = Game.GetStateAs<EditorState>();

			if ( !string.IsNullOrEmpty( state.CurrentFileName ) )
			{
				Input.Text = state.CurrentFileName;
			}

			Input.Focus();
			Input.AddEventListener( "onsubmit", () => HandleSubmit() );
			Input.AddEventListener( "onblur", () => HandleClose() );

			base.PostTemplateApplied();
		}
	}
}
