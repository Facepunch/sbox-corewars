using Facepunch.CoreWars.Blocks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class AxeConfig : WeaponConfig
	{
		public override string ClassName => "weapon_axe";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 30;
	}

	[Library( "weapon_axe" )]
	public partial class Axe : MeleeWeapon
	{
		public override WeaponConfig Config => new AxeConfig();
		public override float ScaleNonBlockDamage => 0.2f;
		public override string ViewModelPath => "models/weapons/axe/v_axe01.vmdl";
		public override bool DoesBlockDamage => true;
		public override DamageFlags DamageType => DamageFlags.Blunt;
		public override float MeleeRange => 80f;
		public override float PrimaryRate => 2f;
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Wooden;
		public override float SecondaryMaterialMultiplier => 0.5f;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/axe/w_axe01.vmdl" );
		}
	}
}
