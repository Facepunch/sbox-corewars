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

		public InventoryContainer Container { get; private set; }
		public List<InventorySlot> Slots { get; private set; }
		public Panel SlotContainer { get; set; }
		public bool IsOpen { get; set; }

		public Backpack()
		{
			Slots = new();
			Current = this;
			AcceptsFocus = true;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			IsOpen = true;
			Focus();
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			IsOpen = false;
			Blur();
		}

		public void SetContainer( InventoryContainer container )
		{
			Container = container;
			SlotContainer.DeleteChildren();
			Slots.Clear();

			for ( ushort i = 0; i < container.SlotLimit; i++ )
			{
				var slot = SlotContainer.AddChild<InventorySlot>();
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
					Slots[i].IsSelected = false;
				}
			}

			base.Tick();
		}

		protected override void OnBlur( PanelEvent e )
		{
			IsOpen = false;

			base.OnBlur( e );
		}

		protected override void PostTemplateApplied()
		{
			if ( Local.Pawn is Player player && player.BackpackInventory.IsValid() )
			{
				SetContainer( player.BackpackInventory.Container );
			}

			BindClass( "hidden", () => !IsOpen );

			base.PostTemplateApplied();
		}
	}
}
