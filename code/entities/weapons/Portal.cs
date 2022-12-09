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
	partial class Portal : Throwable<BulletDropProjectile>
	{
		public override WeaponConfig Config => new PortalConfig();
		public override string TrailEffect => "particles/weapons/portal_grenade/portal_grenade_trail/portal_grenade_trail.vpcf";
		public override string ThrowSound => "portal.launch";
		public override string HitSound => null;
		public override string DamageType => "blast";

		private CoreWarsPlayer PlayerToTeleport { get; set; }

		protected override void OnThrown()
		{
			if ( IsServer && Owner is CoreWarsPlayer player )
			{
				PlayerToTeleport = player;
				EnableDrawing = false;
			}
		}

		protected override void OnProjectileHit( BulletDropProjectile projectile, TraceResult trace )
		{
			if ( IsClient ) return;

			var position = projectile.Position;
			var world = VoxelWorld.Current;
			var blockPosition = world.ToVoxelPosition( position );
			var blockBelowType = world.GetAdjacentBlock( blockPosition, (int)BlockFace.Bottom );

			if ( blockBelowType > 0 )
			{
				var player = PlayerToTeleport;
				if ( !player.IsValid() ) return;

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
				PlayerToTeleport = null;
				HasBeenThrown = false;
				EnableDrawing = true;

				using ( Prediction.Off() )
				{
					PlaySound( "portal.fail" );
				}
			}
		}
	}
}
