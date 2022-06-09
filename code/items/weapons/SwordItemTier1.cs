using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_sword_1" )]
	public class SwordItemTier1 : WeaponItem
	{
		public override string WorldModelPath => "models/weapons/sword/w_sword01.vmdl";
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override string WeaponName => "weapon_sword";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_sword_1.png";
		public override string Name => "Light Sword";
		public override string Group => "sword";
		public override int Tier => 1;
	}
}
