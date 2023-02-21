using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PickaxeConfig : WeaponConfig
	{
		public override string ClassName => "weapon_pickaxe";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 30;
	}

	[Library( "weapon_pickaxe" )]
	public partial class Pickaxe : MeleeWeapon
	{
		public override WeaponConfig Config => new PickaxeConfig();
		public override float ScaleNonBlockDamage => 0.2f;
		public override string ViewModelPath => "models/weapons/pickaxe/v_pickaxe01.vmdl";
		public override bool DoesBlockDamage => true;
		public override string DamageType => "blunt";
		public override float MeleeRange => 80f;
		public override float PrimaryRate => 1.2f;
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Metal;
		public override float SecondaryMaterialMultiplier => 0.5f;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/pickaxe/w_pickaxe01.vmdl" );
		}
	}
}
