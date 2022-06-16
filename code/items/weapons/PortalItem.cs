using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_portal" )]
	public class PortalItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override ItemTag[] Tags => new ItemTag[0];
		public override string Description => "Throwing it will transport you instantly to where it lands.";
		public override string WeaponName => "weapon_portal";
		public override Color Color => ColorPalette.Abilities;
		public override string Icon => "textures/items/weapon_portal.png";
		public override string Name => "Portal";
	}
}
