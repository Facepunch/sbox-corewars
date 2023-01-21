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
			if ( !EnableDrawing || !VoxelWorld.Current.IsValid() )
				return;

			var vb = new VertexBuffer();
			vb.Init( true );

			var center = new Vector3( VoxelWorld.Current.VoxelSize * 0.5f );
			var size = new IntVector3( VoxelWorld.Current.VoxelSize );

			DrawBox( vb, center, size * 1f );
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( VoxelWorld.Current.IsValid() )
			{
				RenderBounds = new BBox( new Vector3( -VoxelWorld.Current.VoxelSize ), new Vector3( VoxelWorld.Current.VoxelSize ) );
			}
		}

		private void DrawBox( VertexBuffer vb, Vector3 center, Vector3 size )
		{
			vb.AddCube( center, size, Rotation.Identity );

			var attributes = new RenderAttributes();

			attributes.Set( "TintColor", Color );
			attributes.Set( "Opacity", 0.8f );

			vb.Draw( BoxMaterial, attributes );
		}
	}
}
