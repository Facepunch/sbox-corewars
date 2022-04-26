using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrowbarShopItemTier4 : BaseWeaponShopItem<CrowbarItemTier4>
	{
		public override string Name => "Crystal Crowbar";
		public override string Description => "A supercharged crowbar for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 60
		};
		public override Type PreviousWeaponType => typeof( CrowbarItemTier3 );
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_crowbar.png";
		}
	}
}
