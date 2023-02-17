using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public abstract partial class Throwable<T> : ProjectileWeapon<T> where T : Projectile, new()
	{
		public override string ViewModelPath => "models/weapons/v_held_item.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => null;
		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;
		public override float InheritVelocity => 0f;
		public override int ClipSize => 0;
		public override float ReloadTime => 2.3f;
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
			if ( Game.IsClient && IsFirstPersonMode )
			{
				projectile.Position = EffectEntity.Position + EffectEntity.Rotation.Forward * 24f + EffectEntity.Rotation.Right * 8f + EffectEntity.Rotation.Down * 4f;
			}
		}

		protected override void OnProjectileHit( T projectile, TraceResult trace )
		{
			if ( Game.IsServer && WeaponItem.IsValid() )
			{
				WeaponItem.Remove();
			}
		}
	}
}
