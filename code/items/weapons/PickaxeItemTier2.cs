using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_pickaxe_2" )]
	public class PickaxeItemTier2 : WeaponItem
	{
		public override string WeaponName => "weapon_pickaxe";
		public override string Description => "A medium pickaxe for breaking defensive blocks.";
		public override string Icon => "textures/items/weapon_pickaxe_2.png";
		public override string Name => "Medium Pickaxe";
		public override string Group => "pickaxe";
		public override int Tier => 2;
	}
}
