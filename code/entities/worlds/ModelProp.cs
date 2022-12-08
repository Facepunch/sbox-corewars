﻿using Facepunch.CoreWars.Editor;

using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

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

		public virtual void OnPlayerSavedData( EditorPlayer player )
		{
			player.LastPlacedEntity.SetEditorData( "model", ModelPath );
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
				SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
			else
				SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}
	}
}
