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
	[EditorEntity( Title = "Team Generator", EditorModel = "models/gameplay/resource_pool/resource_pool.vmdl" )]
	[Category( "Generators" )]
	public partial class TeamGenerator : BaseGenerator
	{
		[EditorProperty, Net] public Team Team { get; set; }

		private TimeUntil NextGenerateCrystalTime { get; set; }
		private TimeUntil NextGenerateGoldTime { get; set; }

		private Particles Effect { get; set; }

		public override void Reset()
		{
			NextGenerateCrystalTime = GetNextGenerateCrystalTime();
			NextGenerateGoldTime = GetNextGenerateGoldTime();

			base.Reset();
		}

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Effect?.Destroy( true );

			Effect = Particles.Create( "particles/gameplay/resource_pool/resource_pool.vpcf", this );
			Effect.SetEntity( 0, this );

			base.ClientSpawn();
		}

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public override void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		protected float GetNextGenerateGoldTime()
		{
			var core = Team.GetCore();

			if ( core.IsValid() )
			{
				var tier = core.GetUpgradeTier( "gold" );

				if ( tier >= 3 )
					return 20f;
				else if ( tier >= 2 )
					return 30f;
			}

			return 40f;
		}

		protected float GetNextGenerateCrystalTime()
		{
			return 30f;
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			RenderColor = Team.GetColor();
		}

		protected override void ServerTick()
		{
			base.ServerTick();
		}

		protected override void GenerateItems()
		{
			Generate<IronItem>( 1 );
		}

		protected override void OnGeneratorTick()
		{
			var core = Team.GetCore();
			if ( !core.IsValid() ) return;

			if ( NextGenerateGoldTime )
			{
				if ( core.HasUpgrade<GoldGeneratorTier1>() )
				{
					Generate<GoldItem>( 1 );
				}

				NextGenerateGoldTime = GetNextGenerateGoldTime();
			}

			if ( NextGenerateCrystalTime )
			{
				if ( core.HasUpgrade<TeamGeneratorTier3>() )
				{
					Generate<CrystalItem>( 1 );
				}

				NextGenerateCrystalTime = GetNextGenerateCrystalTime();
			}
		}

		protected override float GetNextGenerateTime()
		{
			var core = Team.GetCore();

			if ( core.IsValid() )
			{
				var tier = core.GetUpgradeTier( "generator" );

				if ( tier >= 2 )
					return 0.5f;
				else if ( tier >= 1 )
					return 1f;
			}

			return 2f;
		}
	}
}
