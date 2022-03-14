using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorAreaGhost : RenderEntity
	{
		public Material BoxMaterial = Material.Load( "materials/editor/place_block.vmat" );
		public BBox StartBlock { get; set; }
		public BBox EndBlock { get; set; }
		public Color Color { get; set; }

		private BBox LocalBBox { get; set; }
		private BBox WorldBBox { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !EnableDrawing || !VoxelWorld.Current.IsValid() )
				return;

			var vb = Render.GetDynamicVB( true );
			var center = LocalBBox.Center;
			var size = LocalBBox.Size;

			DrawBox( vb, center, size );
		}

		public void UpdateRenderBounds()
		{
			WorldBBox = new BBox( StartBlock.Mins, StartBlock.Maxs );
			WorldBBox = WorldBBox.AddPoint( EndBlock.Mins );
			WorldBBox = WorldBBox.AddPoint( EndBlock.Maxs );

			Position = WorldBBox.Mins;

			var localMins = WorldBBox.Mins - Position;
			var localMaxs = WorldBBox.Maxs - Position;

			LocalBBox = new BBox( localMins, localMaxs );
			RenderBounds = LocalBBox * 10f;
		}

		private void DrawBox( VertexBuffer vb, Vector3 center, Vector3 size )
		{
			vb.AddCube( center, size, Rotation.Identity );

			Render.Attributes.Set( "GhostColor", Color );
			Render.Attributes.Set( "Opacity", 0.8f );

			vb.Draw( BoxMaterial );
		}
	}
}
