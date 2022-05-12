﻿using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Gold Generator", Group = "Generators", EditorModel = "models/gameplay/resource_pool/resource_pool_gold.vmdl" )]
	public class GoldGenerator : BaseGenerator
	{
		private Particles Effect { get; set; }
		private int Tier { get; set; }

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
			return 30f;
		}
	}
}
