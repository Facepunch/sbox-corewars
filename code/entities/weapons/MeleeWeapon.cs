using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public abstract partial class MeleeWeapon : BlockDamageWeapon
	{
		public virtual float DamageStaminaThreshold => 40f;
		public virtual bool ScaleDamageWithStamina => true;
		public virtual float ScaleNonBlockDamage => 1f;
		public virtual float StaminaLossPerSwing => 4f;
		public virtual bool DoesBlockDamage => false;
		public virtual bool UseTierBodyGroups => false;
		public virtual string HitPlayerSound => "melee.hitflesh";
		public virtual string HitObjectSound => "sword.slash";
		public virtual string SwingSound => "melee.swing";
		public virtual float Force => 1.5f;

		public override float MeleeRange => 80f;
		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;

		public override void AttackPrimary()
		{
			if ( Owner is not CoreWarsPlayer player )
				return;

			var damageScale = ScaleNonBlockDamage;

			if ( ScaleDamageWithStamina )
			{
				damageScale *= Math.Max( (player.Stamina / DamageStaminaThreshold ), 1f );
			}

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( SwingSound );
			MeleeStrike( Config.Damage * damageScale, Force );

			if ( Game.IsServer && WeaponItem.IsValid() && DoesBlockDamage )
			{
				DamageVoxelInDirection( MeleeRange, Config.Damage + ( Config.Damage * ( WeaponItem.Tier - 1 ) * 0.75f ) );
			}

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			player.ReduceStamina( StaminaLossPerSwing );
		}

		public override void CreateViewModel()
		{
			Game.AssertClient();

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

		public override void SimulateAnimator( CitizenAnimationHelper anim )
		{
			anim.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
		}

		protected override void OnWeaponItemChanged()
		{
			if ( Game.IsServer && WeaponItem.IsValid() && !string.IsNullOrEmpty( WeaponItem.WorldModelPath ) )
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

			if ( victim is CoreWarsPlayer target )
				target.PlaySound( HitPlayerSound );
			else
				victim.PlaySound( HitObjectSound );

			base.OnMeleeAttackHit( victim );
		}
	}
}
