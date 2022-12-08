
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class SwordItemTier1 : WeaponItem
	{
		public override string WorldModelPath => "models/weapons/sword/w_sword01.vmdl";
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override string UniqueId => "item_sword_1";
		public override string WeaponName => "weapon_sword";
		public override string Description => "A light sword for dealing melee damage.";
		public override string Icon => "textures/items/weapon_sword_1.png";
		public override string Name => "Light Sword";
		public override string Group => "sword";
		public override int Tier => 1;
	}
}
