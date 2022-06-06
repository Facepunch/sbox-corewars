using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class DefuserShopItem : BaseWeaponShopItem<DefuserItem>
	{
		public override string Name => "Defuser";
		public override string Description => "A simple tool for defusing explosives.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_defuser.png";
		}
	}
}
