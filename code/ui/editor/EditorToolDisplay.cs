using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorToolDisplay : Panel
	{
		public string ToolName => GetToolName();
		public string ToolDescription => GetToolDescription();
		public Panel ToolIcon { get; set; }

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
			BindClass( "hidden", () => IsHidden() );
			
			base.PostTemplateApplied();
		}
	}
}
