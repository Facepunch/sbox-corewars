
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public class BaseBrewShopItem<T> : BaseShopItem where T : BrewItem, new()
	{
		public override string Name => ItemDefinition.Name;
		public override IReadOnlySet<string> Tags => ItemDefinition.Tags;
		public override string Description => ItemDefinition.Description;
		public override Color Color => ItemDefinition.Color;
		public override int SortOrder => 7;

		public T ItemDefinition { get; private set; } = new T();

		public override string GetIcon( CoreWarsPlayer player )
		{
			return ItemDefinition.Icon;
		}

		public override void OnPurchased( CoreWarsPlayer player )
		{
			var item = InventorySystem.CreateItem<T>();
			player.TryGiveItem( item );
		}
	}
}
