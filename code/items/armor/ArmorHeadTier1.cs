using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_head_1" )]
	public class ArmorHeadTier1 : ArmorItem
	{
		public override float DamageMultiplier => 0.7f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Head;
		public override string Name => "Hardhat Head Armor";
		public override string Icon => "textures/items/armor_head_1.png";
	}
}
