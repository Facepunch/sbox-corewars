
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class FireballItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override string WeaponName => "weapon_fireball";
		public override string UniqueId => "item_fireball";
		public override Color Color => UI.ColorPalette.Abilities;
		public override string Description => "A fireball which can melt fungus and damage other players.";
		public override string Icon => "textures/items/weapon_fireball.png";
		public override string Name => "Fireball";
	}
}
