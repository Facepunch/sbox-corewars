using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_longsword_3" )]
	public class LongswordItemTier3 : WeaponItem
	{
		public override string WeaponName => "weapon_longsword";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_longsword_3.png";
		public override string Name => "Heavy Longsword";
		public override string Group => "longsword";
		public override int Tier => 3;
	}
}
