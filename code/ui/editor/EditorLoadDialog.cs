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

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorLoadDialog();

			Game.Hud.AddChild( Current );
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			base.Tick();
		}

		protected virtual void HandleSubmit()
		{
			Game.LoadEditorMapCmd( Input.Text );
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
