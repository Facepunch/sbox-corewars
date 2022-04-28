using Facepunch.CoreWars.Inventory;
using System.Linq;
using System;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BaseWeaponShopItem<T> : BaseShopItem where T : WeaponItem, new()
	{
		public T ItemDefinition { get; private set; } = new T();

		public override bool CanPurchase( Player player )
		{
			if ( !base.CanPurchase( player ) ) return false;

			var items = player.FindItems<WeaponItem>()
				.Where( i => i.WeaponName == ItemDefinition.WeaponName );

			if ( items.Any( i => i.WeaponTier >= ItemDefinition.WeaponTier ) )
				return false;

			if ( ItemDefinition.WeaponTier > 1 )
			{
				if ( !items.Any( i => i.WeaponTier == ItemDefinition.WeaponTier - 1 ) )
					return false;
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

			if ( ItemDefinition.WeaponTier > 1 )
			{
				var oldItem = player.FindItems<WeaponItem>()
					.Where( i => i.WeaponName == ItemDefinition.WeaponName )
					.FirstOrDefault();

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
