﻿using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class VolumeEntity : RenderEntity
	{
		public Material Material = Material.Load( "materials/editor/place_block.vmat" );
		public Color Color { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !EnableDrawing || !VoxelWorld.Current.IsValid() )
				return;

			var vb = Render.GetDynamicVB( true );
			var center = RenderBounds.Center;
			var size = RenderBounds.Size;

			DrawBox( vb, center, size );
		}

		private void DrawBox( VertexBuffer vb, Vector3 center, Vector3 size )
		{
			vb.AddCube( center, size + new Vector3( 0.5f ), Rotation.Identity, default, size / VoxelWorld.Current.VoxelSize );

			Render.Attributes.Set( "TintColor", Color );
			Render.Attributes.Set( "Opacity", 0.8f );

			vb.Draw( Material );
		}
	}
}
