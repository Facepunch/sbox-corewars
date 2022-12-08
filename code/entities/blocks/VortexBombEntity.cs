using Facepunch.CoreWars.Editor;

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
		[Net] public bool IsBeingNeutralized { get; private set; }

		private bool HasExploded { get; set; }
		private Particles Effect { get; set; }
		private Sound Sound { get; set; }

		public override void Initialize()
		{
			CenterOnBlock( true, true );
			TimeUntilExplode = 4f;

			base.Initialize();
		}

		public override void ClientSpawn()
		{
			Effect = Particles.Create( "particles/gameplay/vortex_bomb/vortex_bomb_base.vpcf", this );
			Effect.SetEntity( 0, this );

			Sound = PlaySound( "vortexbomb.loop" );

			UpdateEffect();

			base.ClientSpawn();
		}

		[ClientRpc]
		protected async void DoExplodeEffect()
		{
			var explosion = Particles.Create( "particles/gameplay/vortex_bomb/explode/vortex_explode_bomb_base.vpcf" );
			explosion.SetPosition( 0, Position );

			await Task.DelaySeconds( 0.2f );
			Sound.FromWorld( "vortexbomb.beforeexplode", Position );
			Sound.Stop();

			await Task.DelaySeconds( 0.8f );
			Sound.FromWorld( "vortexbomb.explode", Position );
			Effect?.Destroy( true );
		}

		[Event.Tick.Client]
		protected virtual void UpdateEffect()
		{
			if ( !IsBeingNeutralized )
			{
				var fraction = TimeUntilExplode / 4f;
				Effect?.SetPosition( 10, new Vector3( fraction ) );
				Effect?.SetPosition( 11, new Vector3( fraction ) );
			}
			else
			{
				Effect?.SetPosition( 10, new Vector3( 1f ) );
				Effect?.SetPosition( 11, new Vector3( 1f ) );
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			var state = World.GetState<BuildingBlockState>( BlockPosition );

			if ( state.IsValid() && state.LastDamageTime < 1f )
				IsBeingNeutralized = true;
			else
				IsBeingNeutralized = false;

			if ( !TimeUntilExplode || HasExploded || IsBeingNeutralized )
				return;

			DoExplodeEffect();
			DestroyAfter( 1f );

			HasExploded = true;
		}

		protected async void DestroyAfter( float delay )
		{
			await Task.DelaySeconds( delay );

			World.SetBlockOnServer( BlockPosition, 0 );

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
			Effect?.Destroy( true );
			Sound.Stop();

			base.OnDestroy();
		}
	}
}
