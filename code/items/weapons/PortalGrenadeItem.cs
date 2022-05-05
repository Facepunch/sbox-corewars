using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_portal_grenade" )]
	public class PortalGrenadeItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override string WeaponName => "weapon_portal_grenade";
		public override string Icon => "textures/items/portal_grenade.png";
		public override string Name => "Portal Grenade";
	}
}
