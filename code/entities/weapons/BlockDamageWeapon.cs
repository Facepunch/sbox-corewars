using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BlockDamageWeapon : Weapon
	{
		public virtual BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Plastic;
		public virtual float SecondaryMaterialMultiplier => 0.3f;

		protected virtual void DamageVoxel( Voxel voxel, float damage )
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

			var state = world.GetOrCreateState<BuildingBlockState>( voxel.Position );

			if ( !state.IsValid() )
			{
				// We can only damage building block states.
				return;
			}

			state.LastDamageTime = 0f;
			state.Health -= (byte)damage;
			state.IsDirty = true;

			if ( state.Health <= 0 )
			{
				world.SetBlockOnServer( voxel.Position, 0 );
			}
		}

		protected virtual void DamageVoxelInDirection( float range )
		{
			var world = VoxelWorld.Current;

			if ( !world.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position, range ) )
			{
				return;
			}

			var voxel = world.GetVoxel( position );

			if ( voxel.IsValid )
			{
				DamageVoxel( voxel, Config.Damage );
			}
		}
	}
}
