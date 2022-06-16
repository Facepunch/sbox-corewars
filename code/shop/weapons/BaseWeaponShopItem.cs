using Facepunch.CoreWars.Inventory;
using System.Linq;
using System;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BaseWeaponShopItem<T> : BaseShopItem where T : WeaponItem, new()
	{
		public override string Name => ItemDefinition.Name;
		public override ItemTag[] Tags => ItemDefinition.Tags;
		public override string Description => ItemDefinition.Description;
		public override Color Color => ItemDefinition.Color;
		public override int SortOrder => 3;
		public virtual AmmoType AmmoType => AmmoType.None;
		public virtual int AmmoAmount => 0;

		public T ItemDefinition { get; private set; } = new T();

		public override bool CanPurchase( Player player )
		{
			if ( !base.CanPurchase( player ) ) return false;

			if ( !string.IsNullOrEmpty( ItemDefinition.Group ) )
			{
				var items = player.FindItems<WeaponItem>()
					.Where( i => i.Group == ItemDefinition.Group );

				if ( items.Any( i => i.Tier >= ItemDefinition.Tier ) )
					return false;

				if ( ItemDefinition.Tier > 1 )
				{
					if ( !items.Any( i => i.Tier == ItemDefinition.Tier - 1 ) )
						return false;
				}
			}

			return true;
		}

		public override string GetIcon( Player player )
		{
			return ItemDefinition.Icon;
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<T>();

			if ( ItemDefinition.Tier > 1 )
			{
				var oldItem = player.FindItems<WeaponItem>()
					.Where( i => i.Group == ItemDefinition.Group )
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

			if ( AmmoType > AmmoType.None && AmmoAmount > 0 )
			{
				var ammo = InventorySystem.CreateItem<AmmoItem>();
				ammo.AmmoType = AmmoType;
				ammo.StackSize = (ushort)AmmoAmount;
				player.TryGiveItem( ammo );
			}
		}
	}
}
