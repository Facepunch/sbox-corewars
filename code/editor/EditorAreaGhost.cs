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
		public float Orientation { get; set; }

		public override void DoRender( SceneObject sceneObject )
		{
			if ( !EnableDrawing || !VoxelWorld.Current.IsValid() )
				return;

			var vb = Render.GetDynamicVB( true );
			var bbox = LocalBBox;

			DrawBox( vb, bbox.Center, bbox.Size );
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
			var world = VoxelWorld.Current;
			var center = (StartBlock.Mins + (EndBlock.Maxs - StartBlock.Mins) * 0.5f);
			var startCenter = StartBlock.Center.RotateAboutPoint( center, Vector3.Up, Orientation );
			var endCenter = EndBlock.Center.RotateAboutPoint( center, Vector3.Up, Orientation );
			var halfVoxel = Vector3.One * world.VoxelSize * 0.5f;
			var start = new BBox( startCenter )
				.AddPoint( startCenter - halfVoxel )
				.AddPoint( startCenter + halfVoxel );

			var end = new BBox( endCenter )
				.AddPoint( endCenter - halfVoxel )
				.AddPoint( endCenter + halfVoxel );

			var worldBBox = new BBox( start.Mins, start.Maxs );
			worldBBox = worldBBox.AddPoint( end.Mins );
			worldBBox = worldBBox.AddPoint( end.Maxs );

			WorldBBox = worldBBox;
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
