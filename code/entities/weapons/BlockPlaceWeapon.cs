using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public abstract class BlockPlaceWeapon<T> : Weapon where T : BlockType
	{
		public override string ViewModelPath => "models/weapons/v_portal.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;

		public override void AttackPrimary()
		{
			if ( Owner is not Player player ) return;

			if ( IsServer )
			{
				var world = VoxelWorld.Current;
				var block = world.GetBlockType<T>();
				var success = world.SetBlockInDirection( player.EyePosition, player.EyeRotation.Forward, block.BlockId, out var blockPosition, true, 5f, ( position ) =>
				{
					var sourcePosition = world.ToSourcePosition( position );
					return player.CanBuildAt( sourcePosition );
				} );

				if ( success )
				{
					using ( Prediction.Off() )
					{
						var particles = Particles.Create( "particles/gameplay/blocks/block_placed/block_placed.vpcf" );
						particles.SetPosition( 0, world.ToSourcePositionCenter( blockPosition ) );
						particles.SetPosition( 6, player.Team.GetColor() );
						PlaySound( "block.place" );
					}

					OnBlockPlaced( blockPosition );
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

		protected virtual void OnBlockPlaced( IntVector3 position )
		{

		}
	}
}
