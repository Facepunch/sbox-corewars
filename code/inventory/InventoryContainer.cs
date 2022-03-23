using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public class InventoryContainer : IValid
	{
		public delegate void ItemTakenCallback( ushort slot, InventoryItem instance );
		public delegate void ItemGivenCallback( ushort slot, InventoryItem instance );
		public delegate void SlotChangedCallback( ushort slot );

		public event SlotChangedCallback OnSlotChanged;
		public event SlotChangedCallback OnDataChanged;
		public event ItemGivenCallback OnItemGiven;
		public event ItemTakenCallback OnItemTaken;
		public event Action<Client> OnClientClosed;
		public event Action<Client> OnConnectionRemoved;
		public event Action<Client> OnConnectionAdded;
		public event Action OnServerClosed;

		private bool InternalIsDirty;

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public bool IsDirty
		{
			get => InternalIsDirty;

			set
			{
				if ( IsServer )
				{
					if ( InternalIsDirty != value )
					{
						InternalIsDirty = value;

						if ( InternalIsDirty )
						{
							InventorySystem.AddToDirtyList( this );
						}
					}
				}
			}
		}

		public ulong InventoryId { get; private set; }
		public Entity Entity { get; }
		public List<Client> Connections { get; }
		public List<InventoryItem> ItemList { get; }
		public ushort SlotLimit { get; private set; }

		public bool IsValid => true;

		public static InventoryContainer Deserialize( byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					return reader.ReadInventoryContainer();
				}
			}
		}

		public byte[] Serialize()
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.WriteInventoryContainer( this );
					return stream.ToArray();
				}
			}
		}

		public void InvokeDataChanged( ushort slot )
		{
			OnDataChanged?.Invoke( slot );
		}

		public void InvokePlayerClosed( Client client )
		{
			OnClientClosed?.Invoke( client );
		}

		public void InvokeServerClosed()
		{
			OnServerClosed?.Invoke();
		}

		public void SendCloseEvent( Client player )
		{
			if ( IsServer )
			{
				InventorySystem.SendCloseInventoryEvent( To.Single( player ), this );
			}
		}

		public void SendCloseEvent()
		{
			if ( IsServer )
			{
				if ( Connections.Count > 0 )
				{
					InventorySystem.SendCloseInventoryEvent( To.Multiple( Connections ), this );
				}
			}
			else
			{
				InventorySystem.SendCloseInventoryEvent( this );
			}
		}

		public bool IsOccupied( ushort slot )
		{
			return (GetFromSlot( slot ) != null);
		}

		public bool SetSlotLimit( ushort slotLimit )
		{
			if ( slotLimit >= ItemList.Count )
			{
				var difference = slotLimit - ItemList.Count;

				for ( var i = 0; i < difference; i++ )
				{
					ItemList.Add( null );
				}
			}
			else if ( slotLimit < ItemList.Count )
			{
				return false;
			}

			SlotLimit = slotLimit;

			return true;
		}

		public InventoryItem GetItem( ulong itemId )
		{
			if ( itemId == 0 )
			{
				return null;
			}

			for ( int i = 0; i < ItemList.Count; i++ )
			{
				var instance = ItemList[i];

				if ( instance != null && instance.ItemId == itemId )
				{
					return ItemList[i];
				}
			}

			return null;
		}

		public void AddConnection( Client connection )
		{
			if ( !Connections.Contains( connection ) )
			{
				Connections.Add( connection );
				OnConnectionAdded?.Invoke( connection );
			}
		}

		public void RemoveConnection( Client connection )
		{
			if ( Connections.Contains( connection ) )
			{
				Connections.Remove( connection );
				OnConnectionRemoved?.Invoke( connection );
			}
		}

		public bool IsConnected( Client connection )
		{
			return Connections.Contains( connection );
		}

		public InventoryItem GetFromSlot( ushort slot )
		{
			return ItemList[slot];
		}

		public void SetInventoryId( ulong inventoryId )
		{
			InventoryId = inventoryId;
		}

		public void SendDirtyItems()
		{
			if ( Connections.Count > 0 )
			{
				InventorySystem.SendDirtyItemsEvent( To.Multiple( Connections ), this );
			}
		}

		public List<T> FindItems<T>() where T : InventoryItem
		{
			var output = new List<T>();

			for ( int i = 0; i < ItemList.Count; i++ )
			{
				var instance = (ItemList[i] as T);

				if ( instance != null )
				{
					output.Add( instance );
				}
			}

			return output;
		}

		public bool Move( InventoryContainer target, ushort fromSlot, ushort toSlot )
		{
			if ( !IsOccupied( fromSlot ) )
			{
				return false;
			}

			if ( IsClient )
			{
				InventorySystem.SendMoveInventoryEvent( this, target, fromSlot, toSlot );
				return true;
			}

			if ( target.IsOccupied( toSlot ) )
			{
				var fromInstance = ItemList[fromSlot];
				var toInstance = target.ItemList[toSlot];
				var canStack = false;

				fromInstance.Container = target;
				fromInstance.SlotId = toSlot;

				toInstance.Container = this;
				toInstance.SlotId = fromSlot;

				if ( fromInstance.IsSameType( toInstance ) && fromInstance.IsStackable )
				{
					canStack = fromInstance.CanStackWith( toInstance );
				}

				if ( canStack )
				{
					toInstance.StackSize += fromInstance.StackSize;

					target.ItemList[toSlot] = toInstance;
					target.SendGiveEvent( toSlot, toInstance );

					ClearSlot( fromSlot );
				}
				else
				{
					SendTakeEvent( fromSlot, fromInstance );
					target.SendTakeEvent( toSlot, toInstance );

					target.ItemList[toSlot] = fromInstance;
					target.SendGiveEvent( toSlot, fromInstance );

					ItemList[fromSlot] = toInstance;
					SendGiveEvent( fromSlot, toInstance );
				}
			}
			else
			{
				var fromInstance = ItemList[fromSlot];

				fromInstance.SlotId = toSlot;
				fromInstance.Container = target;

				target.ItemList[toSlot] = fromInstance;
				target.SendGiveEvent( toSlot, fromInstance );

				ClearSlot( fromSlot, false );
			}

			return true;
		}

		public InventoryItem Remove( ulong itemId )
		{
			if ( itemId == 0 )
			{
				return null;
			}

			for ( ushort i = 0; i < ItemList.Count; i++ )
			{
				var instance = ItemList[i];

				if ( instance != null && instance.ItemId == itemId )
				{
					return ClearSlot( i );
				}
			}

			return null;
		}

		public InventoryItem ClearSlot( ushort slot, bool clearItemContainer = true )
		{
			if ( IsClient )
			{
				return null;
			}

			if ( !IsOccupied( slot ) )
			{
				return null;
			}

			var instance = GetFromSlot( slot );

			if ( clearItemContainer )
			{
				if ( instance.Container == this )
				{
					instance.Container = null;
					instance.SlotId = 0;
				}
			}

			ItemList[slot] = null;

			SendTakeEvent( slot, instance );

			return instance;
		}

		public List<InventoryItem> Give( List<InventoryItem> instances )
		{
			var remainder = new List<InventoryItem>();

			for ( var i = 0; i < instances.Count; i++ )
			{
				var instance = instances[i];

				if ( !Give( instance ) )
				{
					remainder.Add( instance );
				}
			}

			return remainder;
		}

		public bool FindFreeSlot( out ushort slot )
		{
			var slotLimit = SlotLimit;

			for ( ushort i = 0; i < slotLimit; i++ )
			{
				if ( ItemList[i] == null )
				{
					slot = i;
					return true;
				}
			}

			slot = 0;
			return false;
		}

		public bool Give( InventoryItem instance )
		{
			if ( !FindFreeSlot( out var slot ) )
			{
				Log.Error( "Unable to give an item to this inventory because there is no space!" );
				return false;
			}

			return Give( instance, slot );
		}

		public bool Give( InventoryItem instance, ushort slot )
		{
			if ( IsClient )
			{
				return false;
			}

			var slotLimit = SlotLimit;

			if ( slot >= slotLimit )
			{
				Log.Info( "Unable to give an item to this inventory because slot #" + slot + " is greater than the limit of " + slotLimit );
				return false;
			}

			if ( ItemList[slot] != null )
			{
				Log.Info( "Unable to give an item to this inventory because slot #" + slot + " is occupied!" );
				return false;
			}

			instance.SlotId = slot;
			instance.Container = this;

			ItemList[slot] = instance;

			SendGiveEvent( slot, instance );

			return true;
		}

		public ushort Stack( InventoryItem instance )
		{
			var amount = instance.StackSize;

			for ( int i = 0; i < ItemList.Count; i++ )
			{
				var item = ItemList[i];

				if ( item != null && item.IsSameType( instance ) && item.CanStackWith( instance ) )
				{
					var amountCanStack = (ushort)Math.Max( item.MaxStackSize - item.StackSize, 0 );

					if ( amountCanStack >= amount )
					{
						item.StackSize += amount;
						amount = 0;
					}
					else
					{
						item.StackSize += amountCanStack;
						amount = (ushort)Math.Max( amount - amountCanStack, 0 );
					}

					if ( amount == 0 ) return 0;
				}
			}

			if ( amount > 0 )
			{
				var item = Give( instance );

				if ( item )
				{
					instance.StackSize = amount;
					return 0;
				}
			}

			return amount;
		}

		public List<InventoryItem> RemoveAll()
		{
			var output = new List<InventoryItem>();

			for ( ushort i = 0; i < ItemList.Count; i++ )
			{
				var instance = ClearSlot( i );

				if ( instance != null )
				{
					output.Add( instance );
				}
			}

			return output;
		}

		private void SendGiveEvent( ushort slot, InventoryItem instance )
		{
			if ( IsClient )
			{
				return;
			}

			if ( Connections.Count > 0 )
			{
				InventorySystem.SendGiveItemEvent( To.Multiple( Connections ), this, slot, instance );
			}

			HandleSlotChanged( slot );

			OnItemGiven?.Invoke( slot, instance );
		}

		private void SendTakeEvent( ushort slot, InventoryItem instance )
		{
			if ( IsClient )
			{
				return;
			}

			if ( Connections.Count > 0 )
			{
				InventorySystem.SendTakeItemEvent( To.Multiple( Connections ), this, slot );
			}

			HandleSlotChanged( slot );

			if ( instance != null )
			{
				OnItemTaken?.Invoke( slot, instance );
			}
		}

		private void HandleSlotChanged( ushort slot )
		{
			OnSlotChanged?.Invoke( slot );
		}

		public void ProcessGiveItemEvent( BinaryReader reader )
		{
			var instance = reader.ReadInventoryItem();
			var slot = reader.ReadUInt16();

			instance.Container = this;
			instance.SlotId = slot;

			ItemList[slot] = instance;
			HandleSlotChanged( slot );
			OnItemGiven?.Invoke( slot, instance );
		}

		public void ProcessTakeItemEvent( BinaryReader reader )
		{
			var slot = reader.ReadUInt16();
			var instance = ItemList[slot];

			if ( instance != null )
			{
				if ( instance.Container == this && instance.SlotId == slot )
				{
					instance.Container = null;
					instance.SlotId = 0;
				}

				ItemList[slot] = null;
				HandleSlotChanged( slot );
				OnItemTaken?.Invoke( slot, instance );
			}
		}

		public InventoryContainer( Entity owner )
		{
			ItemList = new List<InventoryItem>();
			Connections = new List<Client>();
			Entity = owner;
		}
	}
}
