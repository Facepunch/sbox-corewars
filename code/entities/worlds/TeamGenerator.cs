using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Generator", Group = "Generators", EditorModel = "models/editor/playerstart.vmdl" )]
	public class TeamGenerator : ModelEntity, ISourceEntity
	{
		[Property] public Team Team { get; set; }

		private TimeUntil NextGeneration { get; set; }

		public override void Spawn()
		{
			SetModel( "models/editor/playerstart.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );

			base.Spawn();
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !Game.TryGetState<GameState>( out var state ) )
			{
				return;
			}

			if ( NextGeneration )
			{
				NextGeneration = 10f;
			}
		}
	}
}
