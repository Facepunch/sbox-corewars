using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crossbow" )]
	public class CrossbowItem : WeaponItem
	{
		public override string WeaponName => "weapon_crossbow";
		public override string Icon => "textures/items/weapon_crossbow.png";
		public override string Name => "Crossbow";
	}
}
