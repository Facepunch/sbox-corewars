using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_axe_2" )]
	public class AxeItemTier2 : WeaponItem
	{
		public override string WeaponName => "weapon_axe";
		public override string Icon => "textures/items/weapon_axe.png";
		public override string Name => "Medium Axe";
		public override int WeaponTier => 2;
	}
}
