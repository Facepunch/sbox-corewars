using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Backpack : Panel
	{
		public static Backpack Current { get; private set; }

		public InventoryContainer BackpackContainer { get; private set; }
		public InventoryContainer EquipmentContainer { get; private set; }
		public List<InventorySlot> BackpackSlots { get; private set; }
		public List<InventorySlot> EquipmentSlots { get; private set; }
		public Panel BackpackSlotRoot { get; set; }
		public Panel EquipmentSlotRoot { get; set; }
		public bool IsOpen { get; set; }

		public Backpack()
		{
			BackpackSlots = new();
			EquipmentSlots = new();
			Current = this;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			IsOpen = true;
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			IsOpen = false;
		}

		public void SetBackpack( InventoryContainer backpack )
		{
			BackpackSlots ??= new();
			BackpackContainer = backpack;
			BackpackSlotRoot.DeleteChildren( true );
			BackpackSlots.Clear();

			for ( ushort i = 0; i < backpack.SlotLimit; i++ )
			{
				var slot = BackpackSlotRoot.AddChild<InventorySlot>();
				slot.Container = backpack;
				slot.Slot = i;
				BackpackSlots.Add( slot );
			}
		}

		public void SetEquipment( InventoryContainer equipment )
		{
			EquipmentSlots ??= new();
			EquipmentContainer = equipment;
			EquipmentSlotRoot.DeleteChildren( true );
			EquipmentSlots.Clear();

			for ( ushort i = 0; i < equipment.SlotLimit; i++ )
			{
				var slot = EquipmentSlotRoot.AddChild<InventorySlot>();

				slot.Container = equipment;
				slot.Slot = i;

				if ( i == 0 )
				{
					slot.SetDefaultIcon( "textures/ui/armor_slot_head.png" );
					slot.SetArmorSlot( ArmorSlot.Head );
				}
				else if ( i == 1 )
				{
					slot.SetDefaultIcon( "textures/ui/armor_slot_chest.png" );
					slot.SetArmorSlot( ArmorSlot.Chest );
				}
				else if ( i == 2 )
				{
					slot.SetDefaultIcon( "textures/ui/armor_slot_legs.png" );
					slot.SetArmorSlot( ArmorSlot.Legs );
				}

				EquipmentSlots.Add( slot );
			}
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player ) return;

			for ( ushort i = 0; i < BackpackSlots.Count; i++)
			{
				var item = BackpackContainer.GetFromSlot( i );

				BackpackSlots[i].SetItem( item );
				BackpackSlots[i].IsSelected = false;
			}

			for ( ushort i = 0; i < EquipmentSlots.Count; i++ )
			{
				var item = EquipmentContainer.GetFromSlot( i );

				EquipmentSlots[i].SetItem( item );
				EquipmentSlots[i].IsSelected = false;
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			if ( Local.Pawn is Player player )
			{
				if ( player.BackpackInventory.IsValid() )
					SetBackpack( player.BackpackInventory.Instance );

				if ( player.EquipmentInventory.IsValid() )
					SetBackpack( player.EquipmentInventory.Instance );
			}

			BindClass( "hidden", () => !IsOpen );
		}
	}
}
