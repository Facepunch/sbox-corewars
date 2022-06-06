using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_defuser" )]
	public class DefuserItem : WeaponItem
	{
		public override string WeaponName => "weapon_defuser";
		public override string Icon => "textures/items/weapon_defuser.png";
		public override string Name => "Defuser";
		public override bool RemoveOnDeath => true;
	}
}
