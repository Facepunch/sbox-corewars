
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class AxeItemTier3 : WeaponItem
	{
		public override string WeaponName => "weapon_axe";
		public override string UniqueId => "item_axe_3";
		public override int WorldModelMaterialGroup => 2;
		public override int ViewModelMaterialGroup => 2;
		public override string WorldModelPath => "models/weapons/axe/w_axe01.vmdl";
		public override string ViewModelPath => "models/weapons/axe/v_axe01.vmdl";
		public override string Description => "A heavy axe for breaking wood.";
		public override Color Color => UI.ColorPalette.Tools;
		public override string Icon => "textures/items/weapon_axe_3.png";
		public override string Name => "Heavy Axe";
		public override string Group => "axe";
		public override int Tier => 3;
	}
}
