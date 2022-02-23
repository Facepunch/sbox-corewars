using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class BoomerConfig : WeaponConfig
	{
		public override string Name => "Boomer";
		public override string Description => "Short-range explosive projectile launcher";
		public override string Icon => "ui/weapons/boomer.png";
		public override string ClassName => "weapon_boomer";
		public override AmmoType AmmoType => AmmoType.Explosive;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Ammo => 10;
		public override int Damage => 500;
	}

	[Library( "weapon_boomer", Title = "Boomer" )]
	partial class Boomer : BulletDropWeapon<BulletDropProjectile>
	{
		public override WeaponConfig Config => new BoomerConfig();
		public override string ImpactEffect => "particles/weapons/boomer/boomer_impact.vpcf";
		public override string TrailEffect => "particles/weapons/boomer/boomer_projectile.vpcf";
		public override string ViewModelPath => "models/weapons/v_shotblast.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => "particles/weapons/boomer/boomer_muzzleflash.vpcf";
		public override string CrosshairClass => "shotgun";
		public override string HitSound => "barage.explode";
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override float PrimaryRate => 0.3f;
		public override float SecondaryRate => 1.0f;
		public override float Speed => 1300f;
		public override float Gravity => 5f;
		public override float InheritVelocity => 0f;
		public override string ProjectileModel => "models/weapons/barage_grenade/barage_grenade.vmdl";
		public override int ClipSize => 1;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 4f;
		public virtual float BlastRadius => 96f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/w_shotblast.vmdl" );
			SetMaterialGroup( 1 );
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
			PlaySound( $"barage.launch" );

			if ( AmmoClip == 0 )
				PlaySound( "blaster.empty" );

			base.AttackPrimary();
		}

		public override void PlayReloadSound()
		{
			PlaySound( "blaster.reload" );
			base.PlayReloadSound();
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 2 );
		}

		protected override float ModifyDamage( Entity victim, float damage )
		{
			if ( victim == Owner ) return damage * 1.25f;

			return base.ModifyDamage( victim, damage );
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, Entity target )
		{
			var explosion = Particles.Create( "particles/weapons/boomer/boomer_explosion.vpcf" );
			explosion.SetPosition( 0, projectile.Position - projectile.Velocity.Normal * projectile.Radius );

			if ( IsServer )
            {
				DamageInRadius( projectile.Position, BlastRadius, Config.Damage, 4f );

				var voxelBlastRadius = (int)(BlastRadius / VoxelWorld.Current.VoxelSize);
				var voxelPosition = VoxelWorld.Current.ToVoxelPosition( projectile.Position );

				for ( var x = -voxelBlastRadius; x <= voxelBlastRadius; ++x )
				{
					for ( var y = -voxelBlastRadius; y <= voxelBlastRadius; ++y )
					{
						for ( var z = -voxelBlastRadius; z <= voxelBlastRadius; ++z )
						{
							var blockPosition = voxelPosition + new IntVector3( x, y, z );

							if ( voxelPosition.Distance( blockPosition ) <= voxelBlastRadius )
							{
								VoxelWorld.Current.SetBlockOnServer( blockPosition, 0, 0 );
							}
						}
					}
				}
			}
		}
	}
}
