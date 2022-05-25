using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrossbowConfig : WeaponConfig
	{
		public override string Name => "Crossbow";
		public override string Description => "Medium-range bolt launcher";
		public override string Icon => "items/weapon_crossbow.png";
		public override string ClassName => "weapon_crossbow";
		public override AmmoType AmmoType => AmmoType.Bolt;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 10;
		public override int Damage => 40;
	}

	[Library( "weapon_crossbow", Title = "Crossbow" )]
	public partial class Crossbow : BulletDropWeapon<BulletDropProjectile>
	{
		public override WeaponConfig Config => new CrossbowConfig();
		public override string ImpactEffect => "particles/weapons/boomer/boomer_impact.vpcf";
		public override string TrailEffect => "particles/weapons/boomer/boomer_projectile.vpcf";
		public override string ViewModelPath => "weapons/rust_crossbow/v_rust_crossbow.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => null;
		public override string HitSound => null;
		public override DamageFlags DamageType => DamageFlags.Bullet;
		public override float PrimaryRate => 0.3f;
		public override float SecondaryRate => 1f;
		public override float Speed => 1300f;
		public override float Gravity => 5f;
		public override float InheritVelocity => 0f;
		public override string ProjectileModel => "weapons/rust_crossbow/rust_crossbow_bolt.vmdl";
		public override int ClipSize => 1;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 4f;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/rust_crossbow/rust_crossbow.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( !TakeAmmo( 1 ) )
			{
				PlaySound( "pistol.dryfire" );
				return;
			}

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"crossbow.fire" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "crossbow.reload" );
			base.PlayReloadSound();
		}

		public override void CreateViewModel()
		{
			base.CreateViewModel();
			ViewModelEntity?.SetAnimParameter( "deploy", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 2 );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, TraceResult trace )
		{
			if ( IsServer && trace.Entity is Player victim )
			{
				var info = new DamageInfo()
					.WithAttacker( Owner )
					.WithWeapon( this )
					.WithPosition( trace.EndPosition )
					.WithForce( projectile.Velocity * 0.1f )
					.WithFlag( DamageType )
					.UsingTraceResult( trace );

				info.Damage = GetDamageFalloff( projectile.StartPosition.Distance( victim.Position ), Config.Damage );
				victim.TakeDamage( info );
			}
		}
	}
}
