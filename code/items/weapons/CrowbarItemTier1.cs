using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crowbar_1" )]
	public class CrowbarItemTier1 : WeaponItem
	{
		public override string WeaponName => "weapon_crowbar";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_crowbar_1.png";
		public override string Name => "Light Crowbar";
		public override string Group => "crowbar";
		public override int Tier => 1;
	}
}
