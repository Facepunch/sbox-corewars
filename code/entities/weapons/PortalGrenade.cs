using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	[Library]
	public class PortalGrenadeConfig : WeaponConfig
	{
		public override string Name => "PortalGrenade";
		public override string Description => "Medium-range bolt launcher";
		public override string Icon => "items/weapon_portal_grenade.png";
		public override string ClassName => "weapon_portal_grenade";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Damage => 40;
	}

	[Library( "weapon_portal_grenade", Title = "PortalGrenade" )]
	partial class PortalGrenade : BulletDropWeapon<BulletDropProjectile>
	{
		public override WeaponConfig Config => new PortalGrenadeConfig();
		public override string ImpactEffect => "particles/weapons/boomer/boomer_impact.vpcf";
		public override string TrailEffect => "particles/weapons/boomer/boomer_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_portal_grenade.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => null;
		public override string HitSound => "barage.explode";
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;
		public override float Speed => 1300f;
		public override float Gravity => 5f;
		public override float InheritVelocity => 0f;
		public override string ProjectileModel => "models/weapons/w_portal_grenade.vmdl";
		public override int ClipSize => 1;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 4f;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/w_portal_grenade.vmdl" );
		}

		public override void AttackPrimary()
		{
			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"barage.launch" );

			if ( Owner is Player player )
			{
				var item = player.GetWeaponItem( this );

				if ( item.IsValid() )
				{
					item.Remove();
				}
			}

			base.AttackPrimary();
		}

		public override void CreateViewModel()
		{
			base.CreateViewModel();
			ViewModelEntity?.SetAnimParameter( "deploy", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 5 );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/boomer/boomer_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Radius );
		}
	}
}
