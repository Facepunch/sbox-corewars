
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorHeadTier3 : ArmorItem
	{
		public override float DamageMultiplier => 0.3f;
		public override string UniqueId => "item_armor_head_3";
		public override ArmorSlot ArmorSlot => ArmorSlot.Head;
		public override string Name => "Heavy Head Armor";
		public override string Description => "A heavy protection head armor piece.";
		public override string Icon => "textures/items/armor_head_3.png";
		public override string PrimaryModel => "models/citizen_clothes/hat/bucket_helmet/models/bucket_helmet.vmdl";
		public override int Tier => 3;
	}
}
