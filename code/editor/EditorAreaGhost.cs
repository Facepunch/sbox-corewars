using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorAreaGhost : RenderEntity
	{
		public Material Material = Material.Load( "materials/editor/place_block.vmat" );
		public BBox StartBlock { get; set; }
		public BBox EndBlock { get; set; }
		public Color Color { get; set; }
		public BBox LocalBBox { get; set; }
		public BBox WorldBBox { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !EnableDrawing || !VoxelWorld.Current.IsValid() )
				return;

			var vb = Render.GetDynamicVB( true );
			var center = LocalBBox.Center;
			var size = LocalBBox.Size;

			DrawBox( vb, center, size );
		}

		public void MoveStartBlock( BBox block )
		{
			var endBlockMinsDelta = (EndBlock.Mins - StartBlock.Mins);
			var endBlockMaxsDelta = (EndBlock.Maxs - StartBlock.Maxs);

			StartBlock = block;
			EndBlock = new BBox( block.Mins + endBlockMinsDelta, block.Maxs + endBlockMaxsDelta );

			UpdateRenderBounds();
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
			vb.AddCube( center, size + new Vector3( 0.5f ), Rotation.Identity, default, size / VoxelWorld.Current.VoxelSize );

			Render.Attributes.Set( "TintColor", Color );
			Render.Attributes.Set( "Opacity", 0.8f );

			vb.Draw( Material );
		}
	}
}
