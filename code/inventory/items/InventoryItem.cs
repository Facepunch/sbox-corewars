using Sandbox;
using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public class InventoryItem : IValid
	{
		public ItemSlot ItemSlot { get; set; } = ItemSlot.Everything;
		public bool IsStackable { get; set; }
		public ushort DefaultStackSize { get; set; } = 1;
		public ushort MaxStackSize { get; set; } = 1;

		private ushort InternalStackSize;
		private bool InternalIsDirty;

		public InventoryContainer Container { get; set; }

		public ushort StackSize
		{
			get => InternalStackSize;

			set
			{
				if ( InternalStackSize != value )
				{
					InternalStackSize = value;
					IsDirty = true;
				}
			}
		}

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;
		public bool IsInstance => ItemId > 0;

		public string UniqueName { get; set; }

		public bool IsDirty
		{
			get => InternalIsDirty;

			set
			{
				if ( IsServer )
				{
					if ( Container == null )
					{
						InternalIsDirty = false;
						return;
					}

					InternalIsDirty = value;

					if ( InternalIsDirty )
					{
						Container.IsDirty = true;
					}
				}
			}
		}

		public bool IsValid => true;
		public ulong ItemId { get; set; }
		public ushort SlotId { get; set; }

		public virtual bool IsSameType( InventoryItem other )
		{
			return (UniqueName == other.UniqueName);
		}

		public virtual bool IsSameType( string other )
		{
			return (UniqueName == other);
		}

		public virtual bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public virtual bool CanStack( int amount )
		{
			return true;
		}

		public virtual string GetName()
		{
			return string.Empty;
		}

		public virtual void Write( BinaryWriter writer )
		{

		}

		public virtual void Read( BinaryReader reader )
		{

		}

		public virtual void OnRemoved()
		{

		}

		public virtual void OnCreated()
		{

		}
	}
}
