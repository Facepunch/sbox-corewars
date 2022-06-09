using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class LongswordShopItemTier4 : BaseWeaponShopItem<LongswordItemTier4>
	{
		public override string Name => "Crystal Longsword";
		public override string Description => "A supercharged longsword for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 60
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_longsword_4.png";
		}
	}
}
