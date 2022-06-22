using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BlockDamageWeapon : Weapon
	{
		public virtual BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Plastic;
		public virtual float SecondaryMaterialMultiplier => 0.3f;

		protected virtual void DamageVoxel( Voxel voxel, float damage, bool showEffects = true )
		{
			var world = VoxelWorld.Current;
			var block = voxel.GetBlockType() as BaseBuildingBlock;

			if ( block == null )
			{
				// We can't damage blocks that aren't building blocks.
				return;
			}

			if ( block.MaterialType == BuildingMaterialType.Unbreakable )
			{
				// We can't damage unbreakable building blocks.
				return;
			}

			damage *= block.DamageMultiplier;

			if ( block.MaterialType != PrimaryMaterialType )
			{
				damage *= SecondaryMaterialMultiplier;
			}

			if ( !string.IsNullOrEmpty( block.HitSound ) )
			{
				using ( Prediction.Off() )
				{
					PlaySound( block.HitSound );
				}
			}

			if ( damage == 0 )
			{
				// We can't do no damage to blocks.
				return;
			}

			var state = world.GetOrCreateState<BuildingBlockState>( voxel.Position );

			if ( !state.IsValid() )
			{
				// We can only damage building block states.
				return;
			}

			var sourcePosition = world.ToSourcePositionCenter( voxel.Position );
			var newHealth = (state.Health - damage).FloorToInt().Clamp( 0, 100 );

			state.LastDamageTime = 0f;
			state.Health = (byte)newHealth;
			state.IsDirty = true;

			if ( state.Health <= 0 )
			{
				using ( Prediction.Off() )
				{
					if ( showEffects )
					{
						var effect = Particles.Create( "particles/gameplay/blocks/block_destroyed/block_destroyed.vpcf" );
						effect.SetPosition( 0, sourcePosition );
					}

					if ( !string.IsNullOrEmpty( block.DestroySound ) )
					{
						PlaySound( block.DestroySound );
					}
				}

				world.SetBlockOnServer( voxel.Position, 0 );

				OnBlockDestroyed( voxel.Position );
			}
			else if ( showEffects )
			{
				using ( Prediction.Off() )
				{
					var effect = Particles.Create( "particles/gameplay/blocks/block_damaged/block_damaged.vpcf" );
					effect.SetPosition( 0, sourcePosition );
				}
			}
		}

		protected virtual void OnBlockDestroyed( IntVector3 position )
		{

		}

		protected virtual void DamageVoxelInDirection( float range, float damage, bool showEffects = true )
		{
			var world = VoxelWorld.Current;

			if ( !world.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position, range / world.VoxelSize ) )
			{
				return;
			}

			var voxel = world.GetVoxel( position );

			if ( voxel.IsValid )
			{
				DamageVoxel( voxel, damage, showEffects );
			}
		}
	}
}
