using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crowbar_2" )]
	public class CrowbarItemTier2 : WeaponItem
	{
		public override string WeaponName => "weapon_crowbar";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_crowbar_2.png";
		public override string Name => "Medium Crowbar";
		public override string Group => "crowbar";
		public override int Tier => 2;
	}
}
