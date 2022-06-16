using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class InventorySlot : Panel, IDraggable, IDroppable, ITooltipProvider
	{
		public InventoryContainer Container { get; set; }
		public ushort Slot { get; set; }
		public InventoryItem Item { get; set; }
		public bool IsSelected { get; set; }
		public string StackSize => GetStackSize();
		public float IconSize => Box.Rect.Size.Length;
		public string DefaultIcon { get; private set; }
		public ArmorSlot ArmorSlot { get; private set; }
		public Action<InventorySlot> OnSelected { get; set; }
		public int DisplaySlot { get; private set; }
		public Panel SlotPanel { get; set; }
		public Label SlotLabel { get; set; }
		public Panel Icon { get; set; }
		public string Description => Item.Description;
		public string Name => Item.Name;
		public Color Color => Item.Color;

		public InventorySlot()
		{
			SlotPanel.SetClass( "hidden", true );
		}

		public void SetItem( InventoryItem item )
		{
			// Early out if we already have this item, there's no need to process this again.
			if ( Item == item )
				return;

			if ( !item.IsValid() )
			{
				if ( !string.IsNullOrEmpty( DefaultIcon ) )
					Icon.Style.SetBackgroundImage( DefaultIcon );
				else
					Icon.Style.BackgroundImage = null;

				Style.SetLinearGradientBackground( Color.Black, 0.5f, new Color( 0.2f ), 0.5f );
				Style.BorderColor = null;

				SetClass( "is-block", false );
				SetClass( "is-empty", true );

				Item = null;

				return;
			}

			var icon = item.Icon;

			if ( !string.IsNullOrEmpty( icon ) )
				Icon.Style.SetBackgroundImage( icon );
			else
				Icon.Style.BackgroundImage = null;

			SlotPanel.SetClass( "hidden", DisplaySlot <= 0 );
			SlotLabel.Text = DisplaySlot.ToString();

			if ( item.Color == Color.White )
				Style.SetLinearGradientBackground( Color.Black, 0.5f, new Color( 0.2f ), 0.5f );
			else
				Style.SetLinearGradientBackground( item.Color, 0.5f, new Color( 0.2f ), 0.5f );

			Style.BorderColor = item.Color.WithAlpha( 0.6f );

			SetClass( "is-block", item is BlockItem );
			SetClass( "is-empty", false );
			Item = item;
		}

		public void SetDisplaySlot( int slot )
		{
			DisplaySlot = slot;
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

		public string GetStackSize()
		{
			if ( !Item.IsValid() ) return string.Empty;

			if ( Item is WeaponItem weaponItem )
			{
				var weapon = weaponItem.Weapon;

				if ( weapon.IsValid() && weapon.ClipSize > 0 )
					return weapon.AmmoClip.ToString();
			}

			return (Item.StackSize > 1) ? Item.StackSize.ToString() : string.Empty;
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

			Audio.Play( "inventory.move" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnSelected?.Invoke( this );
			base.OnClick( e );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			if ( Item.IsValid() )
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

		protected override void OnRightClick( MousePanelEvent e )
		{
			if ( !Item.IsValid() ) return;

			var container = Item.Container;
			if ( container.TransferTargetHandler == null ) return;

			var transferContainer = container.TransferTargetHandler.Invoke( Item );

			if ( transferContainer.IsValid() )
			{
				InventorySystem.SendTransferInventoryEvent( container, transferContainer, Item.SlotId );
				Audio.Play( "inventory.move" );
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
