using Facepunch.CoreWars.Utility;
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
			if ( !EnableDrawing || !VoxelWorld.Current.IsValid() )
				return;

			var vb = new VertexBuffer();
			vb.Init( true );

			var center = VoxelWorld.Current.MaxSize * VoxelWorld.Current.VoxelSize * 0.5f;
			var size = VoxelWorld.Current.MaxSize * (float)VoxelWorld.Current.VoxelSize;

			DrawBox( vb, center, size * 1f );
		}

		private void DrawBox( VertexBuffer vb, Vector3 center, Vector3 size )
		{
			vb.AddCube( center, size, Rotation.Identity, default, size / VoxelWorld.Current.VoxelSize, true );
			vb.AddCube( center, size, Rotation.Identity, default, size / VoxelWorld.Current.VoxelSize );

			var attributes = new RenderAttributes();

			attributes.Set( "Opacity", 0.5f );
			attributes.Set( "Color", Color );

			vb.Draw( BoxMaterial, attributes );
		}
	}
}
