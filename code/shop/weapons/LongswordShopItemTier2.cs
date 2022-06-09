using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class LongswordShopItemTier2 : BaseWeaponShopItem<LongswordItemTier2>
	{
		public override string Name => "Medium Longsword";
		public override string Description => "A medium longsword for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_longsword_2.png";
		}
	}
}
