using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_blowtorch" )]
	public class BlowtorchItem : WeaponItem
	{
		public override string WeaponName => "weapon_blowtorch";
		public override string Icon => "textures/items/weapon_blowtorch.png";
		public override string Name => "Blowtorch";
	}
}
