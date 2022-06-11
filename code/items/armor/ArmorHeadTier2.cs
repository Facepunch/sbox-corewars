using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_head_2" )]
	public class ArmorHeadTier2 : ArmorItem
	{
		public override float DamageMultiplier => 0.5f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Head;
		public override string Name => "Medium Head Armor";
		public override string Description => "A medium protection head armor piece.";
		public override string Icon => "textures/items/armor_head_2.png";
		public override string ModelName => "models/citizen_clothes/hat/hat_securityhelmet.vmdl";
		public override int Tier => 2;
	}
}
