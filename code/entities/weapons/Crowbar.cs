using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrowbarConfig : WeaponConfig
	{
		public override string Name => "Crowbar";
		public override string Description => "Your every day melee weapon";
		public override string Icon => "items/weapon_crowbar.png";
		public override string ClassName => "weapon_crowbar";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 10;
	}

	[Library( "weapon_crowbar", Title = "Crowbar" )]
	partial class Crowbar : Weapon
	{
		public override WeaponConfig Config => new CrowbarConfig();
		public override string ImpactEffect => "particles/weapons/boomer/boomer_impact.vpcf";
		public override string ViewModelPath => "models/weapons/v_crowbar.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => "particles/weapons/boomer/boomer_muzzleflash.vpcf";
		public override DamageFlags DamageType => DamageFlags.Blunt;
		public override float PrimaryRate => 1.5f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 1;
		public override bool IsMelee => true;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/w_crowbar.vmdl" );
		}

		public override void AttackPrimary()
		{
			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"barage.launch" );
			MeleeStrike( Config.Damage, 1.5f );

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
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
			Log.Info( IsClient );

			base.OnMeleeAttackHit( victim );
		}
	}
}
