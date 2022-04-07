using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Generator", Group = "Generators", EditorModel = "models/gameplay/resource_pool/resource_pool.vmdl" )]
	public partial class TeamGenerator : ModelEntity, ISourceEntity
	{
		[EditorProperty, Net] public Team Team { get; set; }

		private TimeUntil NextGeneration { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

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

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			RenderColor = Team.GetColor();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !Game.IsState<GameState>() ) return;

			if ( NextGeneration )
			{
				NextGeneration = 10f;
			}
		}
	}
}
