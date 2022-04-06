using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Gold Generator", Group = "Generators", EditorModel = "models/editor/playerstart.vmdl" )]
	public class GoldGenerator : ModelEntity, ISourceEntity
	{
		public virtual void Serialize( BinaryWriter writer ) { }

		public virtual void Deserialize( BinaryReader reader ) { }

		public override void Spawn()
		{
			SetModel( "models/editor/playerstart.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }
	}
}
