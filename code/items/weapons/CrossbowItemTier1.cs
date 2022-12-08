
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class CrossbowItemTier1 : WeaponItem
	{
		public override string WeaponName => "weapon_crossbow";
		public override string UniqueId => "item_crossbow_1";
		public override string Description => "A light damage crossbow for dealing ranged damage.";
		public override string Icon => "textures/items/weapon_crossbow_1.png";
		public override string Name => "Light Crossbow";
		public override string Group => "crossbow";
		public override int Tier => 1;
	}
}
