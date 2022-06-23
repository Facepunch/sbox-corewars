using Facepunch.CoreWars;
using Facepunch.CoreWars.Utility;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorBlockItem : Panel, IDraggable, ITooltipProvider
	{
		public BlockType BlockType { get; set; }
		public byte BlockId { get; set; }
		public float IconSize => Box.Rect.Size.Length;
		public Panel Icon { get; set; }

		public string Description => BlockType.Description;
		public ItemTag[] Tags { get; set; }
		public string Name => BlockType.FriendlyName;
		public Color Color => Color.White;

		public EditorBlockItem() { }

		public void SetBlockId( byte blockId )
		{
			BlockType = VoxelWorld.Current.GetBlockType( blockId );
			BlockId = blockId;
			Tags = BlockType.GetItemTags();

			var icon = $"textures/blocks/corewars/color/{BlockType.DefaultTexture}.png";

			if ( !string.IsNullOrEmpty( BlockType.Icon ) )
				icon = BlockType.Icon;

			if ( !string.IsNullOrEmpty( icon ) )
			{
				if ( FileSystem.Mounted.FileExists( icon ) )
				{
					Icon.Style.SetBackgroundImage( icon );
					Icon.Style.BackgroundSizeX = Length.Cover;
					Icon.Style.BackgroundSizeY = Length.Cover;
				}
			}
			else
			{
				Icon.Style.BackgroundImage = null;
			}
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			if ( BlockType.IsValid() )
			{
				Tooltip.Show( this );
			}

			base.OnMouseOver( e );
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			Tooltip.Hide( this );
			base.OnMouseOut( e );
		}

		protected override void OnMouseDown( MousePanelEvent e )
		{
			Draggable.Start( this, DraggableMode.Move );
			base.OnMouseDown( e );
		}

		protected override void OnMouseUp( MousePanelEvent e )
		{
			Draggable.Stop( this );
			base.OnMouseUp( e );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}

		public string GetIconTexture()
		{
			var block = VoxelWorld.Current.GetBlockType( BlockId );
			return $"textures/blocks/corewars/color/{ block.DefaultTexture }.png";
		}
	}
}
