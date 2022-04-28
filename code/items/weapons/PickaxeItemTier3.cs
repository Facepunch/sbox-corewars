using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_pickaxe_3" )]
	public class PickaxeItemTier3 : WeaponItem
	{
		public override string WeaponName => "weapon_pickaxe";
		public override string Icon => "textures/items/weapon_pickaxe.png";
		public override string Name => "Heavy Pickaxe";
		public override int WeaponTier => 3;
	}
}
