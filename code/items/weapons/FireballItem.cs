using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_fireball" )]
	public class FireballItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override ItemTag[] Tags => new ItemTag[0];
		public override string WeaponName => "weapon_fireball";
		public override Color Color => ColorPalette.Abilities;
		public override string Description => "A fireball which can melt plastic and damage other players.";
		public override string Icon => "textures/items/weapon_fireball.png";
		public override string Name => "Fireball";
	}
}
