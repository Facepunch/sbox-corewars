using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Model Prop", EditorModel = "models/citizen_props/crate01.vmdl" )]
	[Category( "Scenery" )]
	public partial class ModelProp : ModelEntity, ISourceEntity, IEditorCallbacks
	{
		[EditorProperty] public string ModelPath { get; set; } = "models/citizen_props/crate01.vmdl";

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( ModelPath );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			ModelPath = reader.ReadString();
			UpdateModelFromPath();
		}

		public virtual void OnPropertyChanged( string propertyName )
		{
			UpdateModelFromPath();
		}

		public override void Spawn()
		{
			Transmit = TransmitType.Always;
			UpdateModelFromPath();

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }

		private void UpdateModelFromPath()
		{
			SetModel( ModelPath );

			var isEditorMode = Game.Current.IsEditorMode;

			if ( isEditorMode )
				SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );
			else
				SetupPhysicsFromModel( PhysicsMotionType.Static );
		}
	}
}
