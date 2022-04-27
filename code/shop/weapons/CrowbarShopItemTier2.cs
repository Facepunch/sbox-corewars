using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrowbarShopItemTier2 : BaseWeaponShopItem<CrowbarItemTier2>
	{
		public override string Name => "Medium Crowbar";
		public override string Description => "A medium crowbar for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};
		public override Type PreviousWeaponType => typeof( CrowbarItemTier1 );
		public override Type NextWeaponType => typeof( CrowbarItemTier2 );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_crowbar.png";
		}
	}
}
