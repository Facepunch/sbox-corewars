using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Text.Json;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorPlayer : Sandbox.Player
	{
		[ConVar.ClientData( Name = "HotbarBlocks" )]
		public static string HotbarBlocks { get; set; } = string.Empty;

		[Net, Predicted] public ushort CurrentHotbarIndex { get; private set; }
		[Net] public IList<byte> HotbarBlockIds { get; set; }
		[Net, Change( nameof( OnToolChanged ) )] public EditorTool Tool { get; private set; }

		public byte SelectedBlockId => HotbarBlockIds[CurrentHotbarIndex];

		private EditorBounds EditorBounds { get; set; }

		public EditorPlayer() : base()
		{
			HotbarBlockIds = new List<byte>();

			for ( var i = 0; i < 8; i++ )
			{
				HotbarBlockIds.Add( 1 );
			}
		}

		public EditorPlayer( Client client ) : this()
		{
			var storedHotbarInfo = JsonSerializer.Deserialize<byte[]>( client.GetClientData( "HotbarBlocks" ) );

			if ( storedHotbarInfo != null )
			{
				for ( var i = 0; i < storedHotbarInfo.Length; i++ )
				{
					HotbarBlockIds[i] = storedHotbarInfo[i];
				}
			}

			client.Pawn = this;
		}

		[ServerCmd]
		public static void SetHotbarBlockId( int slot, byte blockId )
		{
			var client = ConsoleSystem.Caller;

			if ( client.Pawn is EditorPlayer player )
			{
				player.HotbarBlockIds[slot] = blockId;
			}
		}

		public void SetActiveTool( EditorTool tool )
		{
			if ( Tool.IsValid() )
			{
				Tool.OnDeselected();
			}

			Tool = tool;
			Tool.Player = this;
			Tool.OnSelected();
		}

		protected virtual void OnToolChanged( EditorTool next, EditorTool previous )
		{
			if ( previous.IsValid() )
			{
				previous.OnDeselected();
			}

			if ( next.IsValid() )
			{
				next.OnSelected();
			}
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

			SetActiveTool( new PlaceBlockTool() );
		}

		public override void Spawn()
		{
			EnableDrawing = false;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				EditorHotbar.Current?.Initialize( HotbarBlockIds.Count );
			}

			EditorBounds = new EditorBounds
			{
				RenderBounds = new BBox( Vector3.One * -10000f, Vector3.One * 10000f ),
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
			}

			if ( Prediction.FirstTime )
			{
				var currentSlotIndex = (int)CurrentHotbarIndex;

				if ( Input.MouseWheel > 0 )
					currentSlotIndex++;
				else if ( Input.MouseWheel < 0 )
					currentSlotIndex--;

				var maxSlotIndex = HotbarBlockIds.Count - 1;

				if ( currentSlotIndex < 0 )
					currentSlotIndex = maxSlotIndex;
				else if ( currentSlotIndex > maxSlotIndex )
					currentSlotIndex = 0;


				CurrentHotbarIndex = (ushort)currentSlotIndex;
			}

			var viewer = Client.Components.Get<ChunkViewer>();
			if ( !viewer.IsValid() ) return;
			if ( viewer.IsInMapBounds() && !viewer.IsCurrentChunkReady ) return;

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );

			Tool?.Simulate( client );
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );
		}

		protected override void OnDestroy()
		{
			EditorBounds?.Delete();
			EditorBounds = null;

			Tool?.OnDeselected();

			base.OnDestroy();
		}
	}
}
