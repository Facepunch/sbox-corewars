using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorHeadShopItemTier1 : BaseArmorShopItem<ArmorHeadTier1>
	{
		public override string Name => "Light Head Armor";
		public override string Description => "A low protection head armor piece.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_head_1.png";
		}
	}
}
