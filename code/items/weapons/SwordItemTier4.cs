using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_sword_4" )]
	public class SwordItemTier4 : WeaponItem
	{
		public override string WorldModelPath => "models/weapons/sword/w_sword04.vmdl";
		public override string ViewModelPath => "models/weapons/sword/v_sword04.vmdl";
		public override string WeaponName => "weapon_sword";
		public override bool RemoveOnDeath => true;
		public override string Icon => "textures/items/weapon_sword_4.png";
		public override string Name => "Crystal Sword";
		public override string Group => "sword";
		public override int Tier => 4;
	}
}
