using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorBlockGhost : RenderEntity
	{
		public Material BoxMaterial = Material.Load( "materials/editor/place_block.vmat" );
		public Color Color { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !EnableDrawing || !Map.Current.IsValid() )
				return;

			var vb = Render.GetDynamicVB( true );
			var center = new Vector3( Map.Current.VoxelSize * 0.5f );
			var size = new IntVector3( Map.Current.VoxelSize );

			DrawBox( vb, center, size * 1f );
		}

		private void DrawBox( VertexBuffer vb, Vector3 center, Vector3 size )
		{
			vb.AddCube( center, size, Rotation.Identity );

			Render.Set( "GhostColor", Color );
			Render.Set( "Opacity", 0.8f );

			vb.Draw( BoxMaterial );
		}
	}
}
