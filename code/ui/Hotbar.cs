using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Hotbar : Panel
	{
		public static Hotbar Current { get; private set; }

		public InventoryContainer Container { get; private set; }
		public List<InventorySlot> Slots { get; private set; }

		public Hotbar()
		{
			Slots = new();
			Current = this;
		}

		public void SetContainer( InventoryContainer container )
		{
			Container = container;

			foreach ( var slot in Slots )
			{
				slot.Delete();
			}

			Slots.Clear();

			for ( ushort i = 0; i < container.SlotLimit; i++ )
			{
				var slot = AddChild<InventorySlot>();
				slot.Container = container;
				slot.Slot = i;
				Slots.Add( slot );
			}
		}

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
			{
				for ( ushort i = 0; i < Slots.Count; i++)
				{
					var item = Container.GetFromSlot( i );

					Slots[i].SetItem( item );
					Slots[i].IsSelected = player.CurrentHotbarIndex == i;
				}
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			if ( Local.Pawn is Player player && player.HotbarInventory.IsValid() )
			{
				SetContainer( player.HotbarInventory.Container );
			}

			base.PostTemplateApplied();
		}
	}
}
