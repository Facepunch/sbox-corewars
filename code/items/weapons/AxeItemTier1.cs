
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class AxeItemTier1 : WeaponItem
	{
		public override string WeaponName => "weapon_axe";
		public override string UniqueId => "item_axe_1";
		public override string WorldModelPath => "models/weapons/axe/w_axe01.vmdl";
		public override string ViewModelPath => "models/weapons/axe/v_axe01.vmdl";
		public override string Icon => "textures/items/weapon_axe_1.png";
		public override string Description => "A light damage axe for breaking wood.";
		public override Color Color => UI.ColorPalette.Tools;
		public override string Name => "Light Axe";
		public override string Group => "axe";
		public override int Tier => 1;
	}
}
