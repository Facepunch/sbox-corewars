using Facepunch.CoreWars.Inventory;
using System;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public abstract class BaseArmorShopItem<T> : BaseShopItem where T : ArmorItem
	{
		public virtual Type PreviousArmorType => null;

		public override bool CanPurchase( Player player )
		{
			if ( !base.CanPurchase( player ) ) return false;

			if ( PreviousArmorType != null )
			{
				var items = player.FindItems( PreviousArmorType );
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

			if ( PreviousArmorType != null )
			{
				var oldItems = player.FindItems( PreviousArmorType );
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
