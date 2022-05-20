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
	[EditorEntity( Title = "Crystal Generator", EditorModel = "models/gameplay/resource_pool/resource_pool_crystal.vmdl" )]
	[Category( "Generators" )]
	public class CrystalGenerator : BaseGenerator, ISourceEntity
	{
		private Particles Effect { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool_crystal.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Effect = Particles.Create( "particles/gameplay/resource_pool/resource_pool_crystal.vpcf", this );
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
			Generate<CrystalItem>( 1 );
		}

		protected override float GetNextGenerateTime()
		{
			if ( Game.TryGetState<GameState>( out var state ) )
			{
				if ( state.HasReachedStage( RoundStage.CrystalIII ) )
					return 15f;
				else if ( state.HasReachedStage( RoundStage.CrystalII ) )
					return 30f;
			}

			return 45f;
		}
	}
}
