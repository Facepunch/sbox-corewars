using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_longsword_4" )]
	public class LongswordItemTier4 : WeaponItem
	{
		public override string WeaponName => "weapon_longsword";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_longsword_4.png";
		public override string Name => "Crystal Longsword";
		public override string Group => "longsword";
		public override int Tier => 4;
	}
}
