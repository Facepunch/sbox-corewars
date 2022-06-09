using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordShopItemTier2 : BaseWeaponShopItem<SwordItemTier2>
	{
		public override string Name => "Medium Sword";
		public override string Description => "A medium sword for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_sword_2.png";
		}
	}
}
