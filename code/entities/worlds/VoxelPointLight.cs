﻿using Facepunch.CoreWars.Editor;

using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Point Light", EditorModel = "models/editor/omni.vmdl" )]
	[Category( "Lighting" )]
	public partial class VoxelPointLight : ModelEntity, ISourceEntity, IEditorCallbacks
	{
		private PointLightEntity Light { get; set; }

		[EditorProperty, Net, Range( 0f, 2048f, 1f )] public float Range { get; set; } = 256f;
		[EditorProperty, Net, Range( 0f, 10f )] public float Brightness { get; set; } = 5f;
		[EditorProperty, Net, Range( 0f, 1f )] public float Red { get; set; } = 1f;
		[EditorProperty, Net, Range( 0f, 1f )] public float Green { get; set; } = 1f;
		[EditorProperty, Net, Range( 0f, 1f )] public float Blue { get; set; } = 1f;

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( Range );
			writer.Write( Red );
			writer.Write( Green );
			writer.Write( Blue );
			writer.Write( Brightness );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Range = reader.ReadSingle();
			Red = reader.ReadSingle();
			Green = reader.ReadSingle();
			Blue = reader.ReadSingle();
			Brightness = reader.ReadSingle();
			UpdateLightSettings();
		}

		public virtual void OnPropertyChanged( string propertyName )
		{
			UpdateLightSettings();
		}

		public virtual void OnPlayerSavedData( EditorPlayer player )
		{

		}

		public override void Spawn()
		{
			var isEditorMode = CoreWarsGame.IsEditorMode;

			SetModel( "models/editor/omni.vmdl" );
			Transmit = isEditorMode ? TransmitType.Always : TransmitType.Never;
			Light = new PointLightEntity();

			UpdateLightSettings();

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }

		protected override void OnDestroy()
		{
			Light?.Delete();
			base.OnDestroy();
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			Light.Transform = Transform;
		}

		private void UpdateLightSettings()
		{
			Light.Range = Range;
			Light.Color = new Color( Red, Green, Blue );
			Light.Brightness = Brightness;

			RenderColor = Light.Color;

			var isEditorMode = CoreWarsGame.IsEditorMode;

			if ( isEditorMode )
				SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
		}
	}
}
