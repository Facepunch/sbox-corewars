using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class LongswordShopItemTier3 : BaseWeaponShopItem<LongswordItemTier3>
	{
		public override string Name => "Heavy Longsword";
		public override string Description => "A heavy longsword for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 70
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_longsword_3.png";
		}
	}
}
