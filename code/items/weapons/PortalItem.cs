
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class PortalItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override string UniqueId => "item_portal";
		public override string Description => "Throwing it will transport you instantly to where it lands.";
		public override string WeaponName => "weapon_portal";
		public override Color Color => UI.ColorPalette.Abilities;
		public override string Icon => "textures/items/weapon_portal.png";
		public override string Name => "Portal";
	}
}
