using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Storage : Panel
	{
		public static Storage Current { get; private set; }

		public InventoryContainer Container { get; private set; }
		public InventoryContainer Backpack { get; private set; }
		public List<InventorySlot> BackpackSlots { get; private set; }
		public List<InventorySlot> StorageSlots { get; private set; }
		public Panel BackpackSlotContainer { get; set; }
		public Panel StorageSlotContainer { get; set; }
		public bool IsOpen { get; set; }
		public Entity Entity { get; private set; }
		public string Name { get; private set; }

		public Storage()
		{
			BackpackSlots = new();
			StorageSlots = new();
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

		public void SetName( string name )
		{
			Name = name;
		}

		public void SetEntity( Entity entity )
		{
			Entity = entity;
		}

		public void SetContainer( InventoryContainer container )
		{
			if ( Local.Pawn is not Player player )
				return;

			BackpackSlots ??= new();
			StorageSlots ??= new();

			BackpackSlotContainer.DeleteChildren( true );
			StorageSlotContainer.DeleteChildren( true );

			BackpackSlots.Clear();
			StorageSlots.Clear();

			Container = container;
			Backpack = player.BackpackInventory.Instance;

			for ( ushort i = 0; i < container.SlotLimit; i++ )
			{
				var slot = StorageSlotContainer.AddChild<InventorySlot>();
				slot.Container = container;
				slot.Slot = i;
				StorageSlots.Add( slot );
			}

			for ( ushort i = 0; i < Backpack.SlotLimit; i++ )
			{
				var slot = BackpackSlotContainer.AddChild<InventorySlot>();
				slot.Container = Backpack;
				slot.Slot = i;
				BackpackSlots.Add( slot );
			}
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			for ( ushort i = 0; i < BackpackSlots.Count; i++ )
			{
				var item = Backpack.GetFromSlot( i );

				BackpackSlots[i].SetItem( item );
				BackpackSlots[i].IsSelected = false;
			}

			for ( ushort i = 0; i < StorageSlots.Count; i++)
			{
				var item = Container.GetFromSlot( i );

				StorageSlots[i].SetItem( item );
				StorageSlots[i].IsSelected = false;
			}

			if ( Entity.IsValid() && Entity is IUsable usable )
			{
				if ( Entity.Position.Distance( player.Position ) >= usable.MaxUseDistance )
				{
					Close();
				}
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			if ( Local.Pawn is Player player && Container.IsValid() )
			{
				SetContainer( Container );
			}

			BindClass( "hidden", () => !IsOpen );
		}
	}
}
