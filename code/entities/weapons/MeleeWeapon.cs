using Facepunch.CoreWars.Blocks;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract partial class MeleeWeapon : BlockDamageWeapon
	{
		public virtual float ScaleNonBlockDamage => 1f;
		public virtual float StaminaLossPerSwing => 5f;
		public virtual bool DoesBlockDamage => false;
		public virtual bool UseTierBodyGroups => false;
		public virtual string HitPlayerSound => "melee.hitflesh";
		public virtual string HitObjectSound => "sword.hit";
		public virtual string SwingSound => "melee.swing";

		public override float MeleeRange => 80f;
		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;

		public override void AttackPrimary()
		{
			if ( Owner is not Player player )
				return;

			if ( player.IsOutOfBreath )
				return;

			PlayAttackAnimation();
			ShootEffects();
			MeleeStrike( Config.Damage * ScaleNonBlockDamage, 1.5f );
			PlaySound( SwingSound );

			if ( IsServer && WeaponItem.IsValid() && DoesBlockDamage )
			{
				DamageVoxelInDirection( MeleeRange, Config.Damage * WeaponItem.Tier );
			}

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			player.ReduceStamina( StaminaLossPerSwing )
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( WeaponItem.IsValid() )
			{
				if ( !string.IsNullOrEmpty( WeaponItem.ViewModelPath ) )
				{
					ViewModelEntity = new ViewModel
					{
						EnableViewmodelRendering = true,
						Position = Position,
						Owner = Owner
					};

					ViewModelEntity.SetModel( WeaponItem.ViewModelPath );
					ViewModelEntity.SetMaterialGroup( WeaponItem.ViewModelMaterialGroup );

					if ( UseTierBodyGroups )
						ViewModelEntity.SetBodyGroup( "tier", WeaponItem.Tier - 1 );

					return;
				}
			}

			base.CreateViewModel();
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

		protected override void OnWeaponItemChanged()
		{
			if ( IsServer && WeaponItem.IsValid() && !string.IsNullOrEmpty( WeaponItem.WorldModelPath ) )
			{
				SetModel( WeaponItem.WorldModelPath );
				SetMaterialGroup( WeaponItem.WorldModelMaterialGroup );

				if ( UseTierBodyGroups )
					SetBodyGroup( "tier", WeaponItem.Tier - 1 );
			}

			base.OnWeaponItemChanged();
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
				PlaySound( HitObjectSound );
			}
		}

		protected override void OnMeleeAttackHit( Entity victim )
		{
			ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );

			if ( victim is Player target )
				target.PlaySound( HitPlayerSound );
			else
				victim.PlaySound( HitObjectSound );

			base.OnMeleeAttackHit( victim );
		}
	}
}
