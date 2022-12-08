
using System;
using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public abstract class BaseArmorShopItem<T> : BaseShopItem where T : ArmorItem, new()
	{
		public T ItemDefinition { get; private set; } = new T();

		public override string Name => ItemDefinition.Name;
		public override string Description => ItemDefinition.Description;
		public override IReadOnlySet<string> Tags => ItemDefinition.Tags;
		public override Color Color => ItemDefinition.Color;
		public override int SortOrder => 6;

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
			return ItemDefinition.Icon;
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
