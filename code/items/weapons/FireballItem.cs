using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_fireball" )]
	public class FireballItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override string WeaponName => "weapon_fireball";
		public override string Icon => "textures/items/weapon_fireball.png";
		public override string Name => "Fireball";
	}
}
