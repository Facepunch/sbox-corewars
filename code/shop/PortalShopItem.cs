using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PortalShopItem : BaseWeaponShopItem<PortalItem>
	{
		public override string Name => "Portal";
		public override string Description => "Throwing it will transport you instantly to where it lands.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_portal.png";
		}
	}
}
