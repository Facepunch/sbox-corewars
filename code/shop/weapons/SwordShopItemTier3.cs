using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordShopItemTier3 : BaseWeaponShopItem<SwordItemTier3>
	{
		public override string Name => "Heavy Sword";
		public override string Description => "A heavy sword for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 70
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_sword_3.png";
		}
	}
}
