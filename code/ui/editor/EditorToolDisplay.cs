using Facepunch.CoreWars.Editor;

using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorToolDisplay : Panel
	{
		public static EditorToolDisplay Current { get; private set; }

		public string ToolName => GetToolName();
		public string ToolDescription => GetToolDescription();
		public Panel HotkeyList { get; set; }
		public Panel ToolIcon { get; set; }

		public EditorToolDisplay()
		{
			Current = this;
		}

		private string GetToolName()
		{
			var tool = GetActiveTool();

			if ( tool.IsValid() )
			{
				var description = TypeLibrary.GetAttribute<EditorToolAttribute>( tool.GetType() );
				var title = description.Title;
				var mode = tool.SecondaryMode;

				if ( !string.IsNullOrEmpty( mode ) )
				{
					return $"{title} ({mode})";
				}

				return title;
			}

			return string.Empty;
		}

		private string GetToolDescription()
		{
			var tool = GetActiveTool();

			if ( tool.IsValid() )
			{
				var description = TypeLibrary.GetAttribute<EditorToolAttribute>( tool.GetType() );
				return description.Description;
			}

			return string.Empty;
		}

		public EditorTool GetActiveTool()
		{
			if ( Local.Pawn is EditorPlayer player )
			{
				if ( player.Tool.IsValid() )
				{
					return player.Tool;
				}
			}

			return default;
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			var tool = GetActiveTool();

			if ( tool.IsValid() )
			{
				var description = TypeLibrary.GetDescription( tool.GetType() );
				ToolIcon.Style.SetBackgroundImage( description.Icon );
			}

			base.Tick();
		}

		public void ClearHotkeys()
		{
			HotkeyList.DeleteChildren( true );
		}

		public void AddHotkey( InputButton button, string text )
		{
			var panel = new Panel();
			panel.AddClass( "item" );

			var glyph = panel.Add.Image( "", "glyph" );
			glyph.Texture = Input.GetGlyph( button, InputGlyphSize.Small );
			glyph.Style.Width = glyph.Texture.Width;
			glyph.Style.Height = glyph.Texture.Height;

			panel.Add.Label( text, "text" );

			HotkeyList.AddChild( panel );
		}

		private bool IsHidden()
		{
			if ( !VoxelWorld.Current.IsValid() )
				return true;

			if ( Local.Pawn is EditorPlayer player )
			{
				if ( player.Tool.IsValid() )
				{
					return false;
				}
			}

			return true;
		}

		protected override void PostTemplateApplied()
		{
			HotkeyList.Parent.BindClass( "hidden", () => HotkeyList.ChildrenCount == 0 );
			BindClass( "hidden", () => IsHidden() );

			base.PostTemplateApplied();
		}
	}
}
