using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PortalGrenadeShopItem : BaseShopItem
	{
		public override string Name => "Portal Grenade";
		public override string Description => "Throwing it will transport you instantly to where it lands.";
		public override int Quantity => 1;
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/portal_grenade.png";
		}

		public override void OnPurchased( Player player )
		{
			
		}
	}
}
