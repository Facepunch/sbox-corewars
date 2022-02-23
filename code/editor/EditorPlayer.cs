using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorPlayer : Sandbox.Player
	{
		public TimeUntil NextBlockPlace { get; private set; }

		private EditorBounds EditorBounds { get; set; }
		private EditorBlockGhost BlockGhost { get; set; }

		public EditorPlayer() : base()
		{

		}

		public EditorPlayer( Client client ) : this()
		{
			client.Pawn = this;
		}

		public virtual void OnMapLoaded()
		{
			EnableHideInFirstPerson = true;
			EnableAllCollisions = false;
			EnableDrawing = true;

			CameraMode = new FirstPersonCamera();

			Controller = new FlyController
			{
				EnableCollisions = false
			};

			Animator = new PlayerAnimator();

			SetModel( "models/citizen/citizen.vmdl" );
		}

		public override void Spawn()
		{
			EnableDrawing = false;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			EditorBounds = new EditorBounds
			{
				RenderBounds = new BBox( Vector3.One * -10000f, Vector3.One * 10000f ),
				EnableDrawing = true,
				Color = Color.Green
			};

			BlockGhost = new EditorBlockGhost
			{
				RenderBounds = new BBox( Vector3.One * -100f, Vector3.One* 100f ),
				EnableDrawing = true,
				Color = Color.Green
			};

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			base.Respawn();
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );
		}

		public override void FrameSimulate( Client client )
		{
			base.FrameSimulate( client );
		}

		public override void Simulate( Client client )
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			if ( IsServer && Prediction.FirstTime )
			{
				if ( Input.Released( InputButton.Attack1 ) && NextBlockPlace )
				{
					var distance = VoxelWorld.Current.VoxelSize * 4f;
					var aimVoxelPosition = VoxelWorld.Current.ToVoxelPosition( Input.Position + Input.Rotation.Forward * distance );
					var face = VoxelWorld.Current.Trace( Input.Position * (1.0f / VoxelWorld.Current.VoxelSize), Input.Rotation.Forward, distance, out var endPosition, out _ );

					if ( face != BlockFace.Invalid && VoxelWorld.Current.GetBlock( endPosition ) != 0 )
					{
						var oppositePosition = VoxelWorld.GetAdjacentPosition( endPosition, (int)face );
						aimVoxelPosition = oppositePosition;
					}

					VoxelWorld.Current.SetBlockOnServer( aimVoxelPosition, VoxelWorld.Current.FindBlockId<GrassBlock>() );
					NextBlockPlace = 0.1f;
				}
				else if ( Input.Released( InputButton.Attack2 ) && NextBlockPlace )
				{
					if ( VoxelWorld.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition ) )
					{
						var voxel = VoxelWorld.Current.GetVoxel( blockPosition );

						if ( voxel.IsValid )
						{
							VoxelWorld.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 );
						}
					}

					NextBlockPlace = 0.1f;
				}
			}

			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var position = currentMap.ToVoxelPosition( Input.Position );
				var voxel = currentMap.GetVoxel( position );

				if ( voxel.IsValid )
				{
					DebugOverlay.ScreenText( 2, $"Sunlight Level: {voxel.GetSunLight()}", 0.1f );
					DebugOverlay.ScreenText( 3, $"Torch Level: ({voxel.GetRedTorchLight()}, {voxel.GetGreenTorchLight()}, {voxel.GetBlueTorchLight()})", 0.1f );
					DebugOverlay.ScreenText( 4, $"Chunk: {voxel.Chunk.Offset}", 0.1f );
					DebugOverlay.ScreenText( 5, $"Position: {position}", 0.1f );
					DebugOverlay.ScreenText( 6, $"Biome: {VoxelWorld.Current.GetBiomeAt( position.x, position.y ).Name}", 0.1f );
				}

				var distance = VoxelWorld.Current.VoxelSize * 4f;
				var aimVoxelPosition = VoxelWorld.Current.ToVoxelPosition( Input.Position + Input.Rotation.Forward * distance );
				var face = VoxelWorld.Current.Trace( Input.Position * (1.0f / VoxelWorld.Current.VoxelSize), Input.Rotation.Forward, distance, out var endPosition, out _ );

				if ( face != BlockFace.Invalid && VoxelWorld.Current.GetBlock( endPosition ) != 0 )
				{
					var oppositePosition = VoxelWorld.GetAdjacentPosition( endPosition, (int)face );
					aimVoxelPosition = oppositePosition;
				}

				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				BlockGhost.Position = aimSourcePosition;
			}

			var viewer = Client.Components.Get<ChunkViewer>();
			if ( !viewer.IsValid() ) return;
			if ( viewer.IsInMapBounds() && !viewer.IsCurrentChunkReady ) return;

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );
		}

		protected override void OnDestroy()
		{
			EditorBounds?.Delete();
			EditorBounds = null;

			base.OnDestroy();
		}
	}
}
