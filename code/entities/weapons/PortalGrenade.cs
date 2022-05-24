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

		private Player PlayerToTeleport { get; set; }

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
				var item = player.GetWeaponItem( this );

				if ( item.IsValid() )
				{
					item.Remove();
				}

				PlayerToTeleport = player;
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
			var explosion = Particles.Create( "particles/weapons/boomer/boomer_explosion.vpcf" );
			explosion.SetPosition( 0, position - projectile.Velocity.Normal * projectile.Radius );

			if ( !PlayerToTeleport.IsValid() )
				return;

			var world = VoxelWorld.Current;
			var blockPosition = world.ToVoxelPosition( position );
			var blockBelowType = world.GetAdjacentBlock( blockPosition, (int)BlockFace.Bottom );

			trace = Trace.Ray( position, position + Vector3.Down * 32f )
				.EntitiesOnly()
				.Run();

			if ( blockBelowType > 0 || trace.Hit )
			{
				PlayerToTeleport.Position = position;
				PlayerToTeleport.ResetInterpolation();
			}
		}
	}
}
