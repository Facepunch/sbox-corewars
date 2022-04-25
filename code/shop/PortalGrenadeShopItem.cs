using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public abstract class PortalGrenadeShopItem : BasePurchasable
	{
		public override string GetIcon( Player player )
		{
			return string.Empty;
		}

		public override void OnPurchased( Player player )
		{
			
		}
	}
}
