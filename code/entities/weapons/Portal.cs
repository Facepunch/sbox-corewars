using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	[Library]
	public class PortalConfig : WeaponConfig
	{
		public override string ClassName => "weapon_portal";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Damage => 40;
	}

	[Library( "weapon_portal" )]
	partial class Portal : BulletDropWeapon<BulletDropProjectile>
	{
		public override WeaponConfig Config => new PortalConfig();
		public override string ImpactEffect => null;
		public override string TrailEffect => "particles/weapons/portal_grenade/portal_grenade_trail/portal_grenade_trail.vpcf";
		public override string ViewModelPath => "models/weapons/v_portal.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => null;
		public override string HitSound => null;
		public override DamageFlags DamageType => DamageFlags.Blast;
		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;
		public override float Speed => 1300f;
		public override float Gravity => 5f;
		public override float InheritVelocity => 0f;
		public override string ProjectileModel => "models/weapons/w_portal.vmdl";
		public override int ClipSize => 0;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 4f;

		[Net, Predicted] private bool HasBeenThrown { get; set; }

		private Player PlayerToTeleport { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/w_portal_grenade.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( HasBeenThrown ) return;

			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"portal.launch" );

			if ( IsServer && Owner is Player player )
			{
				PlayerToTeleport = player;
				EnableDrawing = false;
			}

			HasBeenThrown = true;

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
			HasBeenThrown = false;

			if ( IsClient ) return;

			var position = projectile.Position;
			var player = PlayerToTeleport;

			if ( !player.IsValid() )
				return;

			var world = VoxelWorld.Current;
			var blockPosition = world.ToVoxelPosition( position );
			var blockBelowType = world.GetAdjacentBlock( blockPosition, (int)BlockFace.Bottom );

			if ( blockBelowType > 0 )
			{
				using ( Prediction.Off() )
				{
					var startFx = Particles.Create( "particles/weapons/portal_grenade/portal_spawn/portal_spawn.vpcf" );
					startFx.SetPosition( 0, PlayerToTeleport.Position );
					startFx.AutoDestroy( 3f );

					var endFx = Particles.Create( "particles/weapons/portal_grenade/portal_spawn/portal_spawn.vpcf" );
					endFx.SetPosition( 0, position );
					endFx.AutoDestroy( 3f );

					player.PlaySound( "portal.teleport" );
				}

				player.Position = position;
				player.ResetInterpolation();

				if ( WeaponItem.IsValid() )
				{
					WeaponItem.Remove();
				}
			}
			else
			{
				using ( Prediction.Off() )
				{
					PlaySound( "portal.fail" );
				}

				PlayerToTeleport = null;
				EnableDrawing = true;
			}
		}
	}
}
