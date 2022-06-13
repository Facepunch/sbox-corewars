﻿using Sandbox;
using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public class InventoryItem : IValid
	{
		public InventoryContainer Container { get; set; }
		public ItemEntity WorldEntity { get; private set; }
		public bool IsWorldEntity { get; private set; }
		public string ClassName { get; set; }

		public virtual ushort DefaultStackSize => 1;
		public virtual ushort MaxStackSize => 1;
		public virtual string WorldModel => string.Empty;
		public virtual string Description => string.Empty;
		public virtual bool RemoveOnDeath => false;
		public virtual bool DropOnDeath => false;
		public virtual bool CanBeDropped => true;
		public virtual Color Color => Color.White;
		public virtual string Name => string.Empty;
		public virtual string Icon => string.Empty;

		public static InventoryItem Deserialize( byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					return reader.ReadInventoryItem();
				}
			}
		}

		public byte[] Serialize()
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.WriteInventoryItem( this );
					return stream.ToArray();
				}
			}
		}

		private ushort InternalStackSize;
		private bool InternalIsDirty;

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

		public LibraryAttribute Attribute { get; set; }

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

		public bool IsValid { get; set; }
		public ulong ItemId { get; set; }
		public ushort SlotId { get; set; }

		public void SetWorldEntity( ItemEntity entity )
		{
			WorldEntity = entity;
			IsWorldEntity = entity.IsValid();
			IsDirty = true;
			Remove();
		}

		public void ClearWorldEntity()
		{
			WorldEntity = null;
			IsWorldEntity = false;
			IsDirty = true;
		}

		public void Remove()
		{
			if ( Container.IsValid() )
			{
				Container.Remove( this );
			}
		}

		public void Replace( InventoryItem other )
		{
			if ( Container.IsValid() )
			{
				Container.Replace( SlotId, other );
			}
		}

		public virtual bool IsSameType( InventoryItem other )
		{
			return (GetType() == other.GetType());
		}

		public virtual bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public virtual void Write( BinaryWriter writer )
		{
			if ( WorldEntity.IsValid() )
			{
				writer.Write( true );
				writer.Write( WorldEntity.NetworkIdent );
			}
			else
			{
				writer.Write( false );
			}

		}

		public virtual void Read( BinaryReader reader )
		{
			IsWorldEntity = reader.ReadBoolean();

			if ( IsWorldEntity )
			{
				WorldEntity = (Entity.FindByIndex( reader.ReadInt32() ) as ItemEntity);
			}
		}

		public virtual void OnRemoved()
		{

		}

		public virtual void OnCreated()
		{

		}
	}
}
