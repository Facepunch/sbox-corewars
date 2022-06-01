using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Player Spawnpoint", EditorModel = "models/dev/playerstart_tint.vmdl" )]
	[Category( "Gameplay" )]
	public class PlayerSpawnpoint : ModelEntity, ISourceEntity
	{
		[EditorProperty] public Team Team { get; set; }

		public override void Spawn()
		{
			SetModel( "models/dev/playerstart_tint.vmdl" );

			var isEditorMode = Game.Current.IsEditorMode;

			EnableDrawing = isEditorMode;
			Transmit = isEditorMode ? TransmitType.Always : TransmitType.Never;

			if ( isEditorMode )
			{
				SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
				EnableSolidCollisions = false;
			}

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
	}
}
