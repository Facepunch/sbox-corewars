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
		public List<InventorySlot> Slots { get; private set; }
		public Panel SlotContainer { get; set; }
		public bool IsOpen { get; set; }
		public Entity Entity { get; private set; }
		public string Name { get; private set; }

		public Storage()
		{
			Slots = new();
			Current = this;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			Backpack.Current.Open( true );
			IsOpen = true;
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			Backpack.Current.Close();
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
			if ( Local.Pawn is not Player player )
				return;

			for ( ushort i = 0; i < Slots.Count; i++)
			{
				var item = Container.GetFromSlot( i );

				Slots[i].SetItem( item );
				Slots[i].IsSelected = false;
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
			if ( Local.Pawn is Player player && Container.IsValid() )
			{
				SetContainer( Container );
			}

			BindClass( "hidden", () => !IsOpen );

			base.PostTemplateApplied();
		}
	}
}
