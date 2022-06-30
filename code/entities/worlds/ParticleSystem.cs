using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Particle System", EditorModel = "models/editor/cone_helper.vmdl" )]
	[Category( "Effects" )]
	public partial class ParticleSystem : ModelEntity, ISourceEntity, IEditorCallbacks
	{
		[EditorProperty] public string ParticlePath { get; set; } = "particles/example/int_from_model_example/int_from_model_example.vpcf";

		private Particles Effect { get; set; }

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( ParticlePath );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			ParticlePath = reader.ReadString();
			UpdateParticleFromPath();
		}

		public virtual void OnPropertyChanged( string propertyName )
		{
			UpdateParticleFromPath();
		}

		public virtual void OnPlayerSavedData( EditorPlayer player )
		{
			
		}

		public override void Spawn()
		{
			Transmit = TransmitType.Always;
			UpdateParticleFromPath();

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }

		private void UpdateParticleFromPath()
		{
			Effect?.Destroy( true );
			Effect = Particles.Create( ParticlePath, this );

			var isEditorMode = Game.Current.IsEditorMode;

			if ( isEditorMode )
			{
				SetModel( "models/editor/cone_helper.vmdl" );
				SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
			}
		}
	}
}
