using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crossbow_2" )]
	public class CrossbowItemTier2 : WeaponItem
	{
		public override string WeaponName => "weapon_crossbow";
		public override string Description => "A heavy damage crossbow for dealing ranged damage.";
		public override string Icon => "textures/items/weapon_crossbow_2.png";
		public override string Name => "Heavy Crossbow";
		public override string Group => "crossbow";
		public override int Tier => 2;
	}
}
