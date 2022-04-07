using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Core", Group = "Team Entities", EditorModel = "models/editor/playerstart.vmdl" )]
	public partial class TeamCore : ModelEntity, ISourceEntity, IResettable
	{
		[EditorProperty, Net] public Team Team { get; set; }

		public virtual void Reset()
		{
			LifeState = LifeState.Alive;
			Health = 100f;
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		public override void Spawn()
		{
			SetModel( "models/editor/playerstart.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !info.Attacker.IsValid() || info.Attacker is not Player attacker )
				return;

			if ( attacker.Team == Team )
				return;

			base.TakeDamage( info );
		}

		public override void OnKilled()
		{
			LifeState = LifeState.Dead;
		}
	}
}
