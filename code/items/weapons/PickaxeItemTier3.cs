using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_pickaxe_3" )]
	public class PickaxeItemTier3 : WeaponItem
	{
		public override string WeaponName => "weapon_pickaxe";
		public override string Description => "A heavy pickaxe for breaking defensive blocks.";
		public override string Icon => "textures/items/weapon_pickaxe_3.png";
		public override string Name => "Heavy Pickaxe";
		public override string Group => "pickaxe";
		public override int Tier => 3;
	}
}
