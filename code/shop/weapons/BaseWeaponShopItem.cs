using Facepunch.CoreWars.Inventory;
using System.Linq;
using System;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BaseWeaponShopItem<T> : BaseShopItem where T : WeaponItem
	{
		public virtual Type PreviousWeaponType => null;
		public virtual Type NextWeaponType => null;

		public override bool CanPurchase( Player player )
		{
			if ( !base.CanPurchase( player ) ) return false;

			if ( NextWeaponType != null )
			{
				var items = player.FindItems( NextWeaponType );
				if ( items.Count > 0 ) return false;
			}

			if ( PreviousWeaponType != null )
			{
				var items = player.FindItems( PreviousWeaponType );
				return items.Count > 0;
			}

			return true;
		}

		public override string GetIcon( Player player )
		{
			return string.Empty;
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<T>();

			if ( PreviousWeaponType != null )
			{
				var oldItems = player.FindItems( PreviousWeaponType );
				var oldItem = oldItems.FirstOrDefault();

				if ( oldItem.IsValid() )
				{
					oldItem.Replace( item );
				}
			}
			else
			{
				player.TryGiveItem( item );
			}
		}
	}
}
