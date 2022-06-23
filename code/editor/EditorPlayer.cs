using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorPlayer : Sandbox.Player, INameplate
	{
		[Net, Predicted] public int CurrentHotbarIndex { get; private set; }
		[Net] public IList<byte> HotbarBlockIds { get; set; }
		[Net, Change( nameof( OnToolChanged ) )] public EditorTool Tool { get; private set; }

		public byte SelectedBlockId => HotbarBlockIds[CurrentHotbarIndex];
		public EditorCamera Camera => CameraMode as EditorCamera;

		public ActionHistory<EditorAction> UndoStack { get; private set; }
		public ActionHistory<EditorAction> RedoStack { get; private set; }

		public Dictionary<int,EditorTool> Tools { get; private set; }

		private EditorBounds EditorBounds { get; set; }
		private Nameplate Nameplate { get; set; }

		public string DisplayName => Client.Name;
		public bool IsFriendly => true;
		public Team Team => Team.Orange;

		public EditorPlayer() : base()
		{
			HotbarBlockIds = new List<byte>();

			for ( var i = 0; i < 8; i++ )
			{
				HotbarBlockIds.Add( 0 );
			}
		}

		public EditorPlayer( Client client ) : this()
		{
			UndoStack = new( 20 );
			RedoStack = new( 20 );
			Tools = new();

			client.Pawn = this;
		}

		[ConCmd.Server]
		public static void ChangeToolTo( int libraryId )
		{
			if ( ConsoleSystem.Caller.Pawn is EditorPlayer player )
			{
				if ( !player.Tools.TryGetValue( libraryId, out var tool ) )
				{
					tool = TypeLibrary.Create<EditorTool>( libraryId );
					player.Tools[libraryId] = tool;
				}

				player.SetActiveTool( tool );
			}
		}

		[ConCmd.Server]
		public static void SetHotbarBlockId( int slot, int blockId )
		{
			var client = ConsoleSystem.Caller;

			if ( client.Pawn is EditorPlayer player )
			{
				player.HotbarBlockIds[slot] = (byte)blockId;
			}
		}

		public void Perform( EditorAction action )
		{
			Host.AssertServer();
			UndoStack.Push( action );
			action.Perform();
		}

		public void Undo()
		{
			Host.AssertServer();

			if ( UndoStack.TryPop( out var action ) )
			{
				RedoStack.Push( action );
				action.Undo();
			}
		}

		public void Redo()
		{
			Host.AssertServer();

			if ( RedoStack.TryPop( out var action ) )
			{
				UndoStack.Push( action );
				action.Perform();
			}
		}

		public void SetActiveTool( EditorTool tool )
		{
			Host.AssertServer();

			if ( Tool.IsValid() )
			{
				Tool.OnDeselected();
			}

			Tool = tool;
			Tool.Player = this;
			Tool.OnSelected();
		}

		public void EnterFlyMode()
		{
			Controller = new FlyController
			{
				EnableCollisions = false
			};
		}

		public void EnterWalkMode()
		{
			Controller = new MoveController
			{
				WalkSpeed = 195f,
				SprintSpeed = 375f
			};
		}

		protected virtual void OnToolChanged( EditorTool previous, EditorTool next )
		{
			if ( !IsLocalPawn ) return;

			if ( previous.IsValid() )
			{
				previous.OnDeselected();
			}

			if ( next.IsValid() )
			{
				next.OnSelected();
			}
		}

		public virtual Transform? GetSpawnpoint()
		{
			var world = VoxelWorld.Current;
			if ( !world.IsValid() ) return null;
			return new Transform( world.MaxSize * world.VoxelSize * 0.5f );
		}

		public virtual void OnMapLoaded()
		{
			EnableHideInFirstPerson = true;
			EnableAllCollisions = false;
			EnableDrawing = true;

			CameraMode = new EditorCamera();
			Animator = new PlayerAnimator();

			EnterFlyMode();

			SetModel( "models/citizen/citizen.vmdl" );
			SetActiveTool( new PlaceBlockTool() );

			var validBlocks = new List<BlockType>();
			var blocks = VoxelWorld.Current.BlockTypes.Values;

			foreach ( var blockId in blocks )
			{
				var block = VoxelWorld.Current.GetBlockType( blockId );

				if ( block.ShowInEditor )
				{
					validBlocks.Add( block );
				}
			}

			for ( var i = 0; i < HotbarBlockIds.Count; i++ )
			{
				if ( HotbarBlockIds[i] > 0 ) continue;
				if ( validBlocks.Count == 0 ) break;
				HotbarBlockIds[i] = validBlocks[0].BlockId;
				validBlocks.RemoveAt( 0 );
			}
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

			Nameplate = new Nameplate( this );

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
			var spawnpoint = GetSpawnpoint();

			if ( spawnpoint.HasValue )
			{
				Transform = spawnpoint.Value;
			}

			LifeState = LifeState.Alive;
			Health = 100f;
			Velocity = Vector3.Zero;
			WaterLevel = 0f;

			CreateHull();
			ResetInterpolation();
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

				if ( IsClient && Input.Down( InputButton.Duck ) && Input.Pressed( InputButton.Back ) )
				{
					var state = Game.GetStateAs<EditorState>();

					if ( !string.IsNullOrEmpty( state.CurrentFileName ) )
					{
						Game.SaveEditorMapCmd( state.CurrentFileName );
					}
					else
					{
						EditorSaveDialog.Open();
					}
				}

				if ( IsServer && Input.Down( InputButton.Duck ) )
				{
					if ( Input.Pressed( InputButton.Reload ) )
					{
						Redo();
					}
					else if ( Input.Pressed( InputButton.Menu ) )
					{
						Undo();
					}
				}

				if ( Input.Released( InputButton.Drop ) )
				{
					if ( Controller is FlyController )
						EnterWalkMode();
					else
						EnterFlyMode();
				}

				UpdateHotbarSlotKeys();
			}

			var viewer = Client.Components.Get<ChunkViewer>();
			if ( !viewer.IsValid() ) return;
			if ( viewer.IsInWorld() && !viewer.IsCurrentChunkReady ) return;

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
			if ( IsServer || IsLocalPawn )
			{
				Tool?.OnDeselected();
			}

			EditorBounds?.Delete();
			EditorBounds = null;

			Nameplate?.Delete();

			base.OnDestroy();
		}

		private void UpdateHotbarSlotKeys()
		{
			if ( Input.Pressed( InputButton.Slot1 ) ) CurrentHotbarIndex = 0;
			if ( Input.Pressed( InputButton.Slot2 ) ) CurrentHotbarIndex = 1;
			if ( Input.Pressed( InputButton.Slot3 ) ) CurrentHotbarIndex = 2;
			if ( Input.Pressed( InputButton.Slot4 ) ) CurrentHotbarIndex = 3;
			if ( Input.Pressed( InputButton.Slot5 ) ) CurrentHotbarIndex = 4;
			if ( Input.Pressed( InputButton.Slot6 ) ) CurrentHotbarIndex = 5;
			if ( Input.Pressed( InputButton.Slot7 ) ) CurrentHotbarIndex = 6;
			if ( Input.Pressed( InputButton.Slot8 ) ) CurrentHotbarIndex = 7;
		}
	}
}
