using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;
using System;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorHotbarSlot : Panel, IDroppable, ITooltipProvider
	{
		public ushort Slot { get; set; }
		public byte BlockId { get; set; }
		public bool IsSelected { get; set; }
		public BlockType BlockType { get; set; }
		public Panel SlotPanel { get; set; }
		public Label SlotLabel { get; set; }

		public string Description => BlockType.Description;
		public ItemTag[] Tags { get; set; }
		public string Name => BlockType.FriendlyName;
		public Color Color => Color.White;

		public EditorHotbarSlot() { }

		public void SetBlockId( byte blockId )
		{
			BlockType = VoxelWorld.Current.GetBlockType( blockId );
			BlockId = blockId;
			Tags = BlockType.GetItemTags();

			var displaySlot = Slot + 1;
			SlotPanel.SetClass( "hidden", displaySlot <= 0 );
			SlotLabel.Text = displaySlot.ToString();

			var icon = $"textures/blocks/corewars/color/{BlockType.DefaultTexture}.png";

			if ( !string.IsNullOrEmpty( BlockType.Icon ) )
				icon = BlockType.Icon;

			if ( !string.IsNullOrEmpty( icon ) )
			{
				if ( Util.FileExistsCached( icon ) )
				{
					Style.SetBackgroundImage( icon );
					Style.BackgroundSizeX = Length.Cover;
					Style.BackgroundSizeY = Length.Cover;
					Style.BackgroundTint = BlockType.TintColor;
				}
			}
			else
			{
				Style.BackgroundImage = null;
			}
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "selected", () => IsSelected );
			base.PostTemplateApplied();
		}

		public bool CanDrop( IDraggable draggable, DraggableMode mode )
		{
			return draggable is EditorBlockItem;
		}

		public void OnDrop( IDraggable draggable, DraggableMode mode )
		{
			if ( draggable is EditorBlockItem item )
			{
				EditorPlayer.SetHotbarBlockId( Slot, (int)item.BlockId );
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
	}
}
