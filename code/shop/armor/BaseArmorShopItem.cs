using Facepunch.CoreWars.Inventory;
using System;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public abstract class BaseArmorShopItem<T> : BaseShopItem where T : ArmorItem, new()
	{
		public T ItemDefinition { get; private set; } = new T();

		public override bool CanPurchase( Player player )
		{
			if ( !base.CanPurchase( player ) ) return false;

			var items = player.FindItems<ArmorItem>()
				.Where( i => i.ArmorSlot == ItemDefinition.ArmorSlot );

			if ( items.Any( i => i.Tier >= ItemDefinition.Tier ) )
				return false;

			if ( ItemDefinition.Tier > 1 )
			{
				if ( !items.Any( i => i.Tier == ItemDefinition.Tier - 1 ) )
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

			if ( ItemDefinition.Tier > 1 )
			{
				var oldItem = player.FindItems<ArmorItem>()
					.Where( i => i.ArmorSlot == ItemDefinition.ArmorSlot )
					.FirstOrDefault();

				if ( oldItem.IsValid() )
				{
					oldItem.Replace( item );
				}
			}
			else
			{
				player.TryGiveArmor( item );
			}
		}
	}
}
