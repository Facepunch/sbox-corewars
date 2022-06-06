using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	[Library]
	public class ImpactGrenadeConfig : WeaponConfig
	{
		public override string Name => "Impact Grenade";
		public override string Description => "A grenade which can melt plastic and damage players";
		public override string Icon => "items/weapon_impact_grenade.png";
		public override string ClassName => "weapon_impact_grenade";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Damage => 50;
	}

	[Library( "weapon_impact_grenade", Title = "Impact Grenade" )]
	partial class ImpactGrenadeGrenade : BulletDropWeapon<BulletDropProjectile>
	{
		public override WeaponConfig Config => new ImpactGrenadeConfig();
		public override string ImpactEffect => "particles/weapons/boomer/boomer_impact.vpcf";
		public override string TrailEffect => "particles/weapons/portal_grenade/portal_grenade_trail/portal_grenade_trail.vpcf";
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
		public override int ClipSize => 0;
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

			if ( IsServer && Owner is Player player )
			{
				if ( WeaponItem.IsValid() )
				{
					WeaponItem.Remove();
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

		protected override void OnProjectileHit( BulletDropProjectile projectile, TraceResult trace )
		{
			var position = projectile.Position;
			var world = VoxelWorld.Current;
			var voxelPosition = world.ToVoxelPosition( position );

			var explosion = Particles.Create( "particles/weapons/boomer/boomer_explosion.vpcf" );
			explosion.SetPosition( 0, position - projectile.Velocity.Normal * projectile.Radius );

			DamageInRadius( position, 512f, Config.Damage, 10f );

			foreach ( var blockPosition in world.GetBlocksInRadius( voxelPosition, 256f ) )
			{
				var blockId = world.GetBlock( blockPosition );
				var block = world.GetBlockType( blockId );

				if ( block is BaseBuildingBlock buildingBlock )
				{
					if ( buildingBlock.MaterialType == BuildingMaterialType.Plastic )
					{
						using ( Prediction.Off() )
						{
							var effect = Particles.Create( "particles/gameplay/blocks/block_destroyed/block_destroyed.vpcf" );
							effect.SetPosition( 0, world.ToSourcePositionCenter( blockPosition ) );
						}

						world.SetBlockOnServer( blockPosition, 0 );
					}
				}
			}
		}
	}
}
