using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crowbar_4" )]
	public class CrowbarItemTier4 : WeaponItem
	{
		public override string WeaponName => "weapon_crowbar";
		public override string Icon => "textures/items/weapon_crowbar.png";
		public override string Name => "Crystal Crowbar";
		public override int WeaponTier => 4;
	}
}
