﻿using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorHeadTier1 : ArmorItem
	{
		public override float DamageMultiplier => 0.7f;
		public override string UniqueId => "item_armor_head_1";
		public override ArmorSlot ArmorSlot => ArmorSlot.Head;
		public override string Name => "Light Head Armor";
		public override string Description => "A low protection head armor piece.";
		public override string Icon => "textures/items/armor_head_1.png";
		public override string PrimaryModel => "models/citizen_clothes/hat/balaclava/models/balaclava.vmdl";
		public override int Tier => 1;
	}
}
