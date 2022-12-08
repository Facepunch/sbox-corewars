
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class VortexBombItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override string WeaponName => "weapon_vortex_bomb";
		public override string UniqueId => "item_vortex_bomb";
		public override Color Color => UI.ColorPalette.Abilities;
		public override string Description => "A high damage explosive block which can only be defused with a Neutralizer.";
		public override string Icon => "textures/items/weapon_vortex_bomb.png";
		public override string Name => "Vortex Bomb";
	}
}
