using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Facepunch.CoreWars.Inventory
{
	public static partial class InventorySystem
	{
		public enum NetworkEvent
		{
			SendDirtyItems,
			CloseInventory,
			OpenInventory,
			MoveInventory,
			GiveItem,
			TakeItem
		}

		private static Dictionary<ulong, InventoryContainer> Containers { get; set; } = new();
		private static Dictionary<ulong, InventoryItem> Items { get; set; } = new();
		private static List<InventoryContainer> DirtyList { get; set; } = new();
		private static Queue<ulong> OrphanedItems { get; set; } = new();

		private static ulong NextContainerId { get; set; }
		private static ulong NextItemId { get; set; }

		public static bool IsServer => Host.IsServer;
		public static bool IsClient => Host.IsClient;

		public static void AddToDirtyList( InventoryContainer container )
		{
			DirtyList.Add( container );
		}

		public static ulong Register( InventoryContainer container, ulong inventoryId = 0 )
		{
			if ( inventoryId == 0 )
			{
				inventoryId = NextContainerId++;
			}

			container.SetInventoryId( inventoryId );
			Containers[inventoryId] = container;

			return inventoryId;
		}

		public static List<InventoryItem> Remove( InventoryContainer container, bool destroyItems = false )
		{
			var inventoryId = container.InventoryId;

			if ( Containers.Remove( inventoryId ) )
			{
				container.SendCloseEvent();

				var itemList = container.RemoveAll();

				if ( destroyItems )
				{
					for ( var i = 0; i < itemList.Count; i++ )
					{
						RemoveItem( itemList[i] );
					}
				}

				return itemList;
			}

			return null;
		}

		public static InventoryContainer Find( ulong inventoryId )
		{
			if ( Containers.TryGetValue( inventoryId, out var container ) )
			{
				return container;
			}

			return null;
		}

		public static InventoryItem FindInstance( ulong itemId )
		{
			Items.TryGetValue( itemId, out var instance );
			return instance;
		}

		public static T FindInstance<T>( ulong itemId ) where T : InventoryItem
		{
			return (FindInstance( itemId ) as T);
		}

		public static void RemoveItem( InventoryItem instance )
		{
			var itemId = instance.ItemId;

			if ( Items.Remove( itemId ) )
			{
				instance.Container?.Remove( itemId );
				instance.OnRemoved();
			}
		}

		public static T CreateItem<T>( ulong itemId = 0 ) where T : InventoryItem
		{
			var attribute = Library.GetAttribute( typeof( T ) );
			return (CreateItem( attribute.Identifier, itemId ) as T);
		}

		public static InventoryItem CreateItem( int libraryId, ulong itemId = 0 )
		{
			if ( itemId > 0 && Items.TryGetValue( itemId, out var instance ) )
			{
				return instance;
			}

			if ( itemId == 0 )
			{
				itemId = ++NextItemId;
			}

			instance = Library.TryCreate<InventoryItem>( libraryId );
			instance.ItemId = itemId;
			instance.IsValid = true;
			instance.StackSize = instance.DefaultStackSize;
			instance.LibraryId = libraryId;
			instance.OnCreated();

			Items[itemId] = instance;

			return instance;
		}

		public static void ClientDisconnected( Client client )
		{
			foreach ( var container in Containers.Values )
			{
				if ( container.IsConnected( client ) )
				{
					container.RemoveConnection( client );
				}
			}
		}

		public static void SendOpenInventoryEvent( To to, InventoryContainer container )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( container.InventoryId );
					SendEventDataToClient( to, NetworkEvent.OpenInventory, stream.ToArray() );
				}
			}
		}

		public static void SendCloseInventoryEvent( To to, InventoryContainer container )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( container.InventoryId );
					SendEventDataToClient( to, NetworkEvent.CloseInventory, stream.ToArray() );
				}
			}
		}

		public static void SendMoveInventoryEvent( InventoryContainer from, InventoryContainer to, ushort fromSlot, ushort toSlot )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( fromSlot );
					writer.Write( from.InventoryId );
					writer.Write( toSlot );
					writer.Write( to.InventoryId );
					SendEventDataToServer( NetworkEvent.MoveInventory, Convert.ToBase64String( stream.ToArray() ) );
				}
			}
		}

		public static void SendCloseInventoryEvent( InventoryContainer container )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( container.InventoryId );
					SendEventDataToServer( NetworkEvent.CloseInventory, Convert.ToBase64String( stream.ToArray() ) );
				}
			}
		}

		public static void SendGiveItemEvent( To to, InventoryContainer container, ushort slotId, InventoryItem instance )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( container.InventoryId );
					writer.WriteInventoryItem( instance );
					writer.Write( slotId );
					SendEventDataToClient( to, NetworkEvent.GiveItem, stream.ToArray() );
				}
			}
		}

		public static void SendTakeItemEvent( To to, InventoryContainer container, ushort slotId )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( container.InventoryId );
					writer.Write( slotId );
					SendEventDataToClient( to, NetworkEvent.TakeItem, stream.ToArray() );
				}
			}
		}

		public static void SendDirtyItemsEvent( To to, InventoryContainer container )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( container.InventoryId );

					ushort dirtyCount = 0;
					var itemList = container.ItemList;

					for ( var i = 0; i < itemList.Count; i++ )
					{
						var item = itemList[i];

						if ( item != null && item.IsDirty )
						{
							dirtyCount++;
						}
					}

					writer.Write( dirtyCount );

					for ( var i = 0; i < itemList.Count; i++ )
					{
						var item = itemList[i];

						if ( item != null && item.IsDirty )
						{
							writer.WriteInventoryItem( item );
							item.IsDirty = false;
						}
					}

					SendEventDataToClient( to, NetworkEvent.SendDirtyItems, stream.ToArray() );
				}
			}
		}

		private static void ProcessOpenInventoryEvent( BinaryReader reader )
		{
			// TODO: Open the inventory UI.
			// var container = Find( reader.ReadUInt64() );
		}

		private static void ProcessTakeItemEvent( BinaryReader reader )
		{
			var inventoryId = reader.ReadUInt64();
			var container = Find( inventoryId );
			container?.ProcessTakeItemEvent( reader );
		}

		private static void ProcessGiveItemEvent( BinaryReader reader )
		{
			var inventoryId = reader.ReadUInt64();
			var container = Find( inventoryId );
			container?.ProcessGiveItemEvent( reader );
		}

		private static void ProcessMoveInventoryEvent( BinaryReader reader )
		{
			var fromSlot = reader.ReadUInt16();
			var fromId = reader.ReadUInt64();
			var toSlot = reader.ReadUInt16();
			var toId = reader.ReadUInt64();
			var fromInventory = Find( fromId );
			var toInventory = Find( toId );

			if ( fromInventory == null )
			{
				Log.Error( "Unable to locate inventory by Id #" + fromId );
				return;
			}

			if ( toInventory == null )
			{
				Log.Error( "Unable to locate inventory by Id #" + toId );
				return;
			}

			if ( IsServer )
				fromInventory.Move( toInventory, fromSlot, toSlot );
		}

		private static void ProcessCloseInventoryEvent( BinaryReader reader, Client client = null )
		{
			var container = Find( reader.ReadUInt64() );

			if ( IsServer )
				container?.InvokePlayerClosed( client );
			else
				container?.InvokeServerClosed();
		}

		private static void ProcessSendDirtyItemsEvent( BinaryReader reader )
		{
			var container = Find( reader.ReadUInt64() );
			if ( container == null ) return;

			var itemCount = reader.ReadUInt16();

			for ( var i = 0; i < itemCount; i++ )
			{
				var item = reader.ReadInventoryItem();

				if ( item != null )
				{
					container.InvokeDataChanged( item.SlotId );
				}
			}
		}

		[ServerCmd]
		public static void SendEventDataToServer( NetworkEvent type, string data )
		{
			var decoded = Convert.FromBase64String( data );

			using ( var stream = new MemoryStream( decoded ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					switch ( type )
					{
						case NetworkEvent.CloseInventory:
							ProcessCloseInventoryEvent( reader, ConsoleSystem.Caller );
							break;
						case NetworkEvent.MoveInventory:
							ProcessMoveInventoryEvent( reader );
							break;
					}
				}
			}
		}

		[ClientRpc]
		public static void SendEventDataToClient( NetworkEvent type, byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					switch ( type )
					{
						case NetworkEvent.SendDirtyItems:
							ProcessSendDirtyItemsEvent( reader );
							break;
						case NetworkEvent.CloseInventory:
							ProcessCloseInventoryEvent( reader );
							break;
						case NetworkEvent.OpenInventory:
							ProcessOpenInventoryEvent( reader );
							break;
						case NetworkEvent.MoveInventory:
							ProcessMoveInventoryEvent( reader );
							break;
						case NetworkEvent.GiveItem:
							ProcessGiveItemEvent( reader );
							break;
						case NetworkEvent.TakeItem:
							ProcessTakeItemEvent( reader );
							break;
					}
				}
			}
		}

		[Event.Tick.Server]
		private static void ServerTick()
		{
			for ( var i = DirtyList.Count - 1; i >= 0; i-- )
			{
				var container = DirtyList[i];
				container.SendDirtyItems();
				container.IsDirty = false;
			}

			DirtyList.Clear();
		}

		[Event.Tick]
		private static void CheckOrphanedItems()
		{
			foreach ( var kv in Items )
			{
				if ( !kv.Value.Container.IsValid() )
				{
					OrphanedItems.Enqueue( kv.Key );
					kv.Value.IsValid = false;
				}
			}

			var totalOrphanedItems = 0;

			while ( OrphanedItems.Count > 0 )
			{
				var itemId = OrphanedItems.Dequeue();
				Items.Remove( itemId );
				totalOrphanedItems++;
			}

			if ( totalOrphanedItems > 0 )
			{
				Log.Info( $"Removed {totalOrphanedItems} orphaned items..." );
			}
		}
	}
}
