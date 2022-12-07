using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public abstract partial class Throwable<T> : BulletDropWeapon<T> where T : BulletDropProjectile, new()
	{
		public override string ImpactEffect => null;
		public override string ViewModelPath => "models/weapons/v_held_item.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => null;
		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;
		public override float Speed => 1300f;
		public override float Gravity => 5f;
		public override float InheritVelocity => 0f;
		public override string ProjectileModel => string.Empty;
		public override int ClipSize => 0;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 4f;
		public virtual string ThrowSound => null;

		[Net, Predicted] public bool HasBeenThrown { get; protected set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/w_held_item.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( HasBeenThrown ) return;

			if ( !string.IsNullOrEmpty( ThrowSound ) )
				PlaySound( ThrowSound );

			PlayAttackAnimation();
			ShootEffects();

			HasBeenThrown = true;
			OnThrown();

			base.AttackPrimary();
		}

		public override void CreateViewModel()
		{
			base.CreateViewModel();

			ViewModelEntity?.SetAnimParameter( "deploy", true );
		}

		public override void SimulateAnimator( CitizenAnimationHelper anim )
		{
			anim.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
		}

		public override void ActiveEnd( Entity entity, bool dropped )
		{
			if ( HasBeenThrown && WeaponItem.IsValid() )
			{
				WeaponItem.Remove();
			}

			base.ActiveEnd( entity, dropped );
		}

		[Event.Client.Frame]
		protected virtual void UpdateBodyGroup()
		{
			ViewModelEntity?.SetBodyGroup( "throw", HasBeenThrown ? 1 : 0 );
		}

		protected virtual void OnThrown()
		{

		}

		protected override void ShootEffects()
		{
			base.ShootEffects();

			ViewModelEntity?.SetAnimParameter( "attack", true );
			ViewModelEntity?.SetAnimParameter( "holdtype_attack", 1 );
		}

		protected override void OnProjectileFired( T projectile )
		{
			if ( IsClient && IsFirstPersonMode )
			{
				projectile.Position = EffectEntity.Position + EffectEntity.Rotation.Forward * 24f + EffectEntity.Rotation.Right * 8f + EffectEntity.Rotation.Down * 4f;
			}
		}

		protected override void OnProjectileHit( T projectile, TraceResult trace )
		{
			if ( IsServer && WeaponItem.IsValid() )
			{
				WeaponItem.Remove();
			}
		}
	}
}
