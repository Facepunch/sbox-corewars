using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	[Library]
	public class FireballConfig : WeaponConfig
	{
		public override string ClassName => "weapon_fireball";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Projectile;
		public override int Damage => 50;
	}

	[Library( "weapon_fireball" )]
	public partial class Fireball : Throwable<Projectile>
	{
		public override WeaponConfig Config => new FireballConfig();
		public override string ProjectileData => "cw_fireball";
		public override string DamageType => "blast";
		
		private Particles HandEffect { get; set; }

		public override void ActiveStart( Entity owner )
		{
			base.ActiveStart( owner );

			HandEffect?.Destroy( true );
			HandEffect = Particles.Create( "particles/weapons/fireball/view_fireball/view_fireball.vpcf", EffectEntity, "hold" );
		}

		public override void ActiveEnd( Entity entity, bool dropped )
		{
			HandEffect?.Destroy( true );
			base.ActiveEnd( entity, dropped );
		}

		protected override void OnProjectileFired( Projectile projectile )
		{
			HandEffect?.Destroy( true );
			base.OnProjectileFired( projectile );
		}

		protected override void OnProjectileHit( Projectile projectile, TraceResult trace )
		{
			var position = projectile.Position;
			var explosion = Particles.Create( "particles/weapons/fireball/fireball_explosion.vpcf" );
			explosion.SetPosition( 0, position - projectile.Velocity.Normal * projectile.Data.Radius );

			if ( Game.IsClient ) return;

			var world = VoxelWorld.Current;
			var voxelPosition = world.ToVoxelPosition( position );

			DamageInRadius( position, 512f, Config.Damage, 10f );

			foreach ( var blockPosition in world.GetBlocksInRadius( voxelPosition, 256f ) )
			{
				var blockId = world.GetBlock( blockPosition );
				var block = world.GetBlockType( blockId );

				if ( block is BaseBuildingBlock buildingBlock )
				{
					if ( buildingBlock.MaterialType == BuildingMaterialType.Fungus )
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

			base.OnProjectileHit( projectile, trace );
		}
	}
}
