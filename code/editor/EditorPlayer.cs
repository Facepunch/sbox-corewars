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

			Camera = new FirstPersonCamera();

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
			if ( !Map.Current.IsValid() ) return;

			if ( IsServer && Prediction.FirstTime )
			{
				if ( Input.Released( InputButton.Attack1 ) && NextBlockPlace )
				{
					var distance = Map.Current.VoxelSize * 4f;
					var aimVoxelPosition = Map.Current.ToVoxelPosition( Input.Position + Input.Rotation.Forward * distance );
					var face = Map.Current.Trace( Input.Position * (1.0f / Map.Current.VoxelSize), Input.Rotation.Forward, distance, out var endPosition, out _ );

					if ( face != BlockFace.Invalid && Map.Current.GetBlock( endPosition ) != 0 )
					{
						var oppositePosition = Map.GetAdjacentPosition( endPosition, (int)face );
						aimVoxelPosition = oppositePosition;
					}

					Map.Current.SetBlockOnServer( aimVoxelPosition, Map.Current.FindBlockId<GrassBlock>() );
					NextBlockPlace = 0.1f;
				}
				else if ( Input.Released( InputButton.Attack2 ) && NextBlockPlace )
				{
					if ( Map.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition ) )
					{
						var voxel = Map.Current.GetVoxel( blockPosition );

						if ( voxel.IsValid )
						{
							Map.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 );
						}
					}

					NextBlockPlace = 0.1f;
				}
			}

			var currentMap = Map.Current;

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
					DebugOverlay.ScreenText( 6, $"Biome: {Map.Current.GetBiomeAt( position.x, position.y ).Name}", 0.1f );
				}

				var distance = Map.Current.VoxelSize * 4f;
				var aimVoxelPosition = Map.Current.ToVoxelPosition( Input.Position + Input.Rotation.Forward * distance );
				var face = Map.Current.Trace( Input.Position * (1.0f / Map.Current.VoxelSize), Input.Rotation.Forward, distance, out var endPosition, out _ );

				if ( face != BlockFace.Invalid && Map.Current.GetBlock( endPosition ) != 0 )
				{
					var oppositePosition = Map.GetAdjacentPosition( endPosition, (int)face );
					aimVoxelPosition = oppositePosition;
				}

				var aimSourcePosition = Map.Current.ToSourcePosition( aimVoxelPosition );

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
