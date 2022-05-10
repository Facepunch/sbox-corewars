using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	public partial class BaseTrigger : ModelEntity, ISourceEntity, IVolumeEntity
	{
		[Net] public Vector3 Mins { get; set; }
		[Net] public Vector3 Maxs { get; set; }

		private VolumeEntity Volume { get; set; }

		public override void Spawn()
		{
			var isEditorMode = Game.Current.IsEditorMode;

			EnableDrawing = isEditorMode;
			Transmit = isEditorMode ? TransmitType.Always : TransmitType.Never;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			var isEditorMode = Game.Current.IsEditorMode;

			if ( isEditorMode )
			{
				var attribute = Library.GetAttribute( GetType() ) as EditorEntityAttribute;

				Volume = new VolumeEntity
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.White
				};

				if ( attribute != null )
				{
					Volume.Material = Material.Load( attribute.VolumeMaterial );
				}
			}

			base.ClientSpawn();
		}

		public virtual void SetVolume( Vector3 mins, Vector3 maxs )
		{
			Mins = mins;
			Maxs = maxs;

			SetupPhysicsFromAABB( PhysicsMotionType.Static, Transform.PointToLocal( mins ), Transform.PointToLocal( maxs ) );

			var isEditorMode = Game.Current.IsEditorMode;

			if ( !isEditorMode )
			{
				CollisionGroup = CollisionGroup.Trigger;
			}

			Tags.Add( "volume" );

			EnableSolidCollisions = false;
			EnableTouch = true;
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( Mins );
			writer.Write( Maxs );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Mins = reader.ReadVector3();
			Maxs = reader.ReadVector3();

			SetVolume( Mins, Maxs );
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			var isEditorMode = Game.Current.IsEditorMode;

			if ( isEditorMode && Volume.IsValid() )
			{
				Volume.Position = Position;
				Volume.RenderBounds = new BBox( Transform.PointToLocal( Mins ), Transform.PointToLocal( Maxs ) );
			}
		}

		protected override void OnDestroy()
		{
			Volume?.Delete();
			Volume = null;

			base.OnDestroy();
		}
	}
}
