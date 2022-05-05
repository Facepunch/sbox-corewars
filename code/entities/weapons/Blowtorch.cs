using Facepunch.CoreWars.Blocks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class BlowtorchConfig : WeaponConfig
	{
		public override string Name => "Blowtorch";
		public override string Description => "For quickly melting plastic";
		public override string Icon => "items/weapon_blowtorch.png";
		public override string ClassName => "weapon_blowtorch";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 20;
	}

	[Library( "weapon_blowtorch", Title = "Blowtorch" )]
	public partial class Blowtorch : BlockDamageWeapon
	{
		public override WeaponConfig Config => new BlowtorchConfig();
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
		public override DamageFlags DamageType => DamageFlags.Burn;
		public override float PrimaryRate => 5f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 1;
		public override bool IsMelee => true;
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Plastic;
		public override float SecondaryMaterialMultiplier => 0f;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void AttackPrimary()
		{
			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"barage.launch" );

			if ( IsServer && WeaponItem.IsValid() )
			{
				DamageVoxelInDirection( 150f, Config.Damage );
			}

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 5 );
			anim.SetAnimParameter( "aim_body_weight", 1.0f );

			if ( Owner.IsValid() )
			{
				ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
				ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
			}
		}

		protected override void ShootEffects()
		{
			base.ShootEffects();

			ViewModelEntity?.SetAnimParameter( "attack", true );
			ViewModelEntity?.SetAnimParameter( "holdtype_attack", 1 );
		}

		protected override void OnMeleeAttackHit( Entity victim )
		{
			ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
			base.OnMeleeAttackHit( victim );
		}
	}
}
