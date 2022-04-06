﻿using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public partial class ItemEntity : ModelEntity
	{
		[Net] public NetInventoryItem Item { get; private set; }

		public void SetItem( InventoryItem item )
		{
			if ( string.IsNullOrEmpty( item.WorldModel ) )
			{
				throw new Exception( "Unable to create an item entity without a world model!" );
			}

			SetModel( item.WorldModel );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			Item = new NetInventoryItem( item );
			item.SetWorldEntity( this );
		}

		public InventoryItem Take()
		{
			var item = Item.Instance;

			if ( item.IsValid() && item.WorldEntity == this )
			{
				item.ClearWorldEntity();
				Delete();
			}

			return Item.Instance;
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( Item.Instance.IsValid() )
			{
				Log.Info( Item.Instance.GetName() );
			}
		}

		public override void Spawn()
		{
			base.Spawn();
		}
	}
}
