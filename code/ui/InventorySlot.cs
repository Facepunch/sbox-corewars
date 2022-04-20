using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class InventorySlot : Panel, IDraggable, IDroppable
	{
		public InventoryContainer Container { get; set; }
		public ushort Slot { get; set; }
		public InventoryItem Item { get; set; }
		public bool IsSelected { get; set; }
		public string StackSize => (Item.IsValid() && Item.StackSize > 1) ? Item.StackSize.ToString() : string.Empty;
		public float IconSize => Box.Rect.Size.Length;

		public InventorySlot() { }

		public void SetItem( InventoryItem item )
		{
			Item = item;

			if ( !item.IsValid() )
			{
				Style.BackgroundImage = null;
				return;
			}

			var icon = item.GetIcon();

			if ( !string.IsNullOrEmpty( icon ) )
			{
				Style.SetBackgroundImage( icon );
				Style.BackgroundSizeX = Length.Cover;
				Style.BackgroundSizeY = Length.Cover;
			}
			else
			{
				Style.BackgroundImage = null;
			}
		}

		protected override void OnRightClick( MousePanelEvent e )
		{
			if ( !Item.IsValid() ) return;

			var container = Item.Container;
			var transferContainer = container.TransferContainer;
			
			if ( transferContainer.IsValid() )
			{
				InventorySystem.SendTransferInventoryEvent( container, transferContainer, Item.SlotId );
			}

			base.OnRightClick( e );
		}

		protected override void OnMouseDown( MousePanelEvent e )
		{
			if ( !Item.IsValid() || e.Button == "mouseright" )
				return;

			Draggable.Start( this, Input.Down( InputButton.Run ) ? DraggableMode.Split : DraggableMode.Move );

			base.OnMouseDown( e );
		}

		protected override void OnMouseUp( MousePanelEvent e )
		{
			Draggable.Stop( this );

			base.OnMouseUp( e );
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "selected", () => IsSelected );
			base.PostTemplateApplied();
		}

		public string GetIconTexture()
		{
			return Item.IsValid() ? Item.GetIcon() : null;
		}

		public bool CanDrop( IDraggable draggable, DraggableMode mode )
		{
			if ( draggable is not InventorySlot slot ) return false;
			if ( slot.Item == Item ) return false;
			return true;
		}

		public void OnDrop( IDraggable draggable, DraggableMode mode )
		{
			if ( draggable is not InventorySlot slot ) return;

			if ( mode == DraggableMode.Move )
				InventorySystem.SendMoveInventoryEvent( slot.Container, Container, slot.Slot, Slot );
			else
				InventorySystem.SendSplitInventoryEvent( slot.Container, Container, slot.Slot, Slot );
		}
	}
}
