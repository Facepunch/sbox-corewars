﻿
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class SwordItemTier4 : WeaponItem
	{
		public override string WorldModelPath => "models/weapons/sword/w_sword01.vmdl";
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override int WorldModelMaterialGroup => 1;
		public override int ViewModelMaterialGroup => 1;
		public override string WeaponName => "weapon_sword";
		public override string UniqueId => "item_sword_4";
		public override bool RemoveOnDeath => true;
		public override string Description => "A supercharged sword for dealing melee damage.";
		public override string Icon => "textures/items/weapon_sword_4.png";
		public override string Name => "Crystal Sword";
		public override string Group => "sword";
		public override int Tier => 4;
	}
}
