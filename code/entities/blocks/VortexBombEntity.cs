using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	[Library]
	[ClassName( "cw_vortex_bomb" )]
	public partial class VortexBombEntity : BlockEntity
	{
		[Net] public RealTimeUntil TimeUntilExplode { get; private set; }

		private Particles Effect { get; set; }
		private Sound Sound { get; set; }

		public override void Initialize()
		{
			CenterOnBlock( true, false );
			TimeUntilExplode = 4f;

			base.Initialize();
		}

		public override void ClientSpawn()
		{
			Effect = Particles.Create( "particles/gameplay/vortex_bomb/vortex_bomb.vpcf", this );
			Effect.SetEntity( 0, this );

			Sound = PlaySound( "explosives.tick" );

			UpdateEffect();

			base.ClientSpawn();
		}

		[Event.Tick.Client]
		protected virtual void UpdateEffect()
		{
			Log.Info( TimeUntilExplode / 4f );
			Effect?.SetPosition( 10, new Vector3( TimeUntilExplode / 4f, 0f, 0f ) );
			Effect?.SetPosition( 11, new Vector3( TimeUntilExplode / 4f, 0f, 0f ) );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !TimeUntilExplode ) return;

			var explosion = Particles.Create( "particles/explosion.vpcf" );
			explosion.SetPosition( 0, Position );

			World.SetBlockOnServer( BlockPosition, 0 );
			Sound.FromWorld( "barage.explode", Position );

			for ( var i = 0; i < 6; i++ )
			{
				var adjacentPosition = VoxelWorld.GetAdjacentPosition( BlockPosition, i );
				var adjacentBlockId = World.GetBlock( adjacentPosition );
				var adjacentBlock = World.GetBlockType( adjacentBlockId );

				if ( adjacentBlock is BaseBuildingBlock buildingBlock )
				{
					if ( buildingBlock.MaterialType != BuildingMaterialType.Blastproof
						&& buildingBlock.MaterialType != BuildingMaterialType.Unbreakable )
					{
						var sourcePosition = World.ToSourcePositionCenter( adjacentPosition );
						var effect = Particles.Create( "particles/gameplay/blocks/block_destroyed/block_destroyed.vpcf" );
						effect.SetPosition( 0, sourcePosition );
						World.SetBlockOnServer( adjacentPosition, 0 );
					}
				}
			}
		}

		protected override void OnDestroy()
		{
			Effect?.Destroy();
			Sound.Stop();

			base.OnDestroy();
		}
	}
}
