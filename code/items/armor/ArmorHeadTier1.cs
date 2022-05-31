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
		public override string Name => "Light Head Armor";
		public override string Icon => "textures/items/armor_head_1.png";
		public override string ModelName => "models/citizen_clothes/hat/balaclava/models/balaclava.vmdl";
		public override int Tier => 1;
	}
}
