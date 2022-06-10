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
		public override int Damage => 10;
	}

	[Library( "weapon_axe" )]
	public partial class Axe : BlockDamageWeapon
	{
		public override WeaponConfig Config => new AxeConfig();
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override DamageFlags DamageType => DamageFlags.Blunt;
		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Wooden;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/sword/w_sword01.vmdl" );
		}

		public override void AttackPrimary()
		{
			PlayAttackAnimation();
			ShootEffects();
			MeleeStrike( Config.Damage * 0.2f, 1.5f );
			PlaySound( "melee.swing" );

			if ( IsServer && WeaponItem.IsValid() )
			{
				DamageVoxelInDirection( 100f, Config.Damage * WeaponItem.Tier );
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

		protected override void OnMeleeAttackMissed( TraceResult trace )
		{
			if ( trace.Hit )
			{
				PlaySound( "sword.hit" );
			}
		}

		protected override void OnMeleeAttackHit( Entity victim )
		{
			ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
			base.OnMeleeAttackHit( victim );
		}
	}
}
