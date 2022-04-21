using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
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
		public string DefaultIcon { get; private set; }
		public ArmorSlot ArmorSlot { get; private set; }
		public Action<InventorySlot> OnSelected { get; set; }
		public Panel Icon { get; set; }

		public InventorySlot() { }

		public void SetItem( InventoryItem item )
		{
			Item = item;

			if ( !item.IsValid() )
			{
				if ( !string.IsNullOrEmpty( DefaultIcon ) )
					Icon.Style.SetBackgroundImage( DefaultIcon );
				else
					Icon.Style.BackgroundImage = null;

				return;
			}

			var icon = item.Icon;

			if ( !string.IsNullOrEmpty( icon ) )
				Icon.Style.SetBackgroundImage( icon );
			else
				Icon.Style.BackgroundImage = null;
		}

		public void SetArmorSlot( ArmorSlot slot )
		{
			ArmorSlot = slot;
		}

		public void SetDefaultIcon( string icon )
		{
			DefaultIcon = icon;
		}

		public string GetIconTexture()
		{
			return Item.IsValid() ? Item.Icon : null;
		}

		public bool CanDrop( IDraggable draggable, DraggableMode mode )
		{
			if ( draggable is not InventorySlot slot ) return false;
			if ( slot.Item == Item ) return false;

			if ( ArmorSlot != ArmorSlot.None )
			{
				if ( slot.Item is not ArmorItem armor )
					return false;

				if ( armor.ArmorSlot != ArmorSlot )
					return false;
			}

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

		protected override void OnClick( MousePanelEvent e )
		{
			OnSelected?.Invoke( this );
			base.OnClick( e );
		}

		protected override void OnRightClick( MousePanelEvent e )
		{
			if ( !Item.IsValid() ) return;

			var container = Item.Container;
			var transferContainer = container.TransferTarget;

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
	}
}
