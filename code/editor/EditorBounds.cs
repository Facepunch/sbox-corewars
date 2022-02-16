using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorBounds : RenderEntity
	{
		public Material BoxMaterial = Material.Load( "materials/editor/bounds.vmat" );
		public Color Color { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !EnableDrawing || !Map.Current.IsValid() )
				return;

			var vb = Render.GetDynamicVB( true );
			var center = Map.Current.MaxSize * Map.Current.VoxelSize * 0.5f;
			var size = Map.Current.MaxSize * (float)Map.Current.VoxelSize;

			DrawBox( vb, center, size * 1f );
		}

		private void DrawBox( VertexBuffer vb, Vector3 center, Vector3 size )
		{
			vb.AddCube( center, size, Rotation.Identity );

			Render.Set( "Opacity", 0.2f );
			Render.Set( "Color", Color );

			vb.Draw( BoxMaterial );
		}
	}
}
