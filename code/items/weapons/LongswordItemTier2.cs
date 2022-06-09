using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_longsword_2" )]
	public class LongswordItemTier2 : WeaponItem
	{
		public override string WeaponName => "weapon_longsword";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_longsword_2.png";
		public override string Name => "Medium Longsword";
		public override string Group => "longsword";
		public override int Tier => 2;
	}
}
