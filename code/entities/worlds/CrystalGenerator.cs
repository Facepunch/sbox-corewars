﻿using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Crystal Generator", Group = "Generators", EditorModel = "models/gameplay/resource_pool/resource_pool_crystal.vmdl" )]
	public class CrystalGenerator : ModelEntity, ISourceEntity
	{
		public virtual void Serialize( BinaryWriter writer ) { }

		public virtual void Deserialize( BinaryReader reader ) { }

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool_crystal.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }
	}
}
