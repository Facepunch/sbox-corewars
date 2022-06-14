using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Gold Generator",EditorModel = "models/gameplay/resource_pool/resource_pool_gold.vmdl" )]
	[Category( "Generators" )]
	public class GoldGenerator : BaseGenerator
	{
		protected override string HudIconPath => "textures/items/gold.png";
		protected override bool ShowHudIcon => true;

		private Particles Effect { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool_gold.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Effect = Particles.Create( "particles/gameplay/resource_pool/resource_pool_gold.vpcf", this );
			Effect.SetEntity( 0, this );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }

		protected override void ServerTick()
		{
			base.ServerTick();
		}

		protected override void GenerateItems()
		{
			Generate<GoldItem>( 1 );
		}

		protected override float GetNextGenerateTime()
		{
			if ( Game.TryGetState<GameState>( out var state ) )
			{
				if ( state.HasReachedStage( RoundStage.GoldIII ) )
					return 10f;
				else if ( state.HasReachedStage( RoundStage.GoldII ) )
					return 20f;
			}

			return 30f;
		}
	}
}
