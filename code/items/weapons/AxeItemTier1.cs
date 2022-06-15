using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_axe_1" )]
	public class AxeItemTier1 : WeaponItem
	{
		public override string WeaponName => "weapon_axe";
		public override string Icon => "textures/items/weapon_axe_1.png";
		public override string Description => "A light damage axe for breaking wood.";
		public override Color Color => ColorPalette.Tools;
		public override string Name => "Light Axe";
		public override string Group => "axe";
		public override int Tier => 1;
	}
}
