using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_pickaxe_2" )]
	public class PickaxeItemTier2 : WeaponItem
	{
		public override string WeaponName => "weapon_pickaxe";
		public override int WorldModelMaterialGroup => 1;
		public override int ViewModelMaterialGroup => 1;
		public override string WorldModelPath => "models/weapons/pickaxe/w_pickaxe01.vmdl";
		public override string ViewModelPath => "models/weapons/pickaxe/v_pickaxe01.vmdl";
		public override string Description => "A medium pickaxe for breaking defensive blocks.";
		public override Color Color => ColorPalette.Tools;
		public override string Icon => "textures/items/weapon_pickaxe_2.png";
		public override string Name => "Medium Pickaxe";
		public override string Group => "pickaxe";
		public override int Tier => 2;
	}
}
