using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public abstract class BaseArmorShopItem<T> : BasePurchasable where T : ArmorItem
	{
		public virtual Type PreviousArmorType => null;

		public override bool CanPurchase( Player player )
		{
			if ( PreviousArmorType != null )
			{
				var items = player.FindItems( PreviousArmorType );
				return items.Count > 0;
			}

			return base.CanPurchase( player );
		}

		public override string GetIcon( Player player )
		{
			return string.Empty;
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<T>();
			player.TryGiveItem( item );
		}
	}
}
