using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_vortex_bomb" )]
	public class VortexBombItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override ItemTag[] Tags => new ItemTag[0];
		public override string WeaponName => "weapon_vortex_bomb";
		public override Color Color => ColorPalette.Abilities;
		public override string Description => "A high damage explosive block which can only be defused with a Neutralizer.";
		public override string Icon => "textures/items/weapon_vortex_bomb.png";
		public override string Name => "Vortex Bomb";
	}
}
