
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class PickaxeItemTier1 : WeaponItem
	{
		public override string WeaponName => "weapon_pickaxe";
		public override string UniqueId => "item_pickaxe_1";
		public override string WorldModelPath => "models/weapons/pickaxe/w_pickaxe01.vmdl";
		public override string ViewModelPath => "models/weapons/pickaxe/v_pickaxe01.vmdl";
		public override string Description => "A light damage pickaxe for breaking defensive blocks.";
		public override Color Color => UI.ColorPalette.Tools;
		public override string Icon => "textures/items/weapon_pickaxe_1.png";
		public override string Name => "Light Pickaxe";
		public override string Group => "pickaxe";
		public override int Tier => 1;
	}
}
