using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crowbar_4" )]
	public class CrowbarItemTier4 : WeaponItem
	{
		public override string WeaponName => "weapon_crowbar";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_crowbar_4.png";
		public override string Name => "Crystal Crowbar";
		public override string Group => "crowbar";
		public override int Tier => 4;
	}
}
