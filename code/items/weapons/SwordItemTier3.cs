using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_sword_3" )]
	public class SwordItemTier3 : WeaponItem
	{
		public override string WorldModelPath => "models/weapons/sword/w_sword01.vmdl";
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override string WeaponName => "weapon_sword";
		public override bool RemoveOnDeath => true;
		public override ItemTag[] Tags => new ItemTag[0];
		public override string Description => "A heavy sword for dealing melee damage.";
		public override string Icon => "textures/items/weapon_sword_3.png";
		public override string Name => "Heavy Sword";
		public override string Group => "sword";
		public override int Tier => 3;
	}
}
