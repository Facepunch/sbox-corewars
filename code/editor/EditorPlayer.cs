using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using Facepunch.CoreWars.UI;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorPlayer : BasePlayer, INameplate
	{
		[Net, Predicted] public int CurrentHotbarIndex { get; private set; }
		[Net] public EditorLastEntityData LastPlacedEntity { get; set; } = new();
		[Net] public IList<byte> HotbarBlockIds { get; set; }
		[Net, Change( nameof( OnToolChanged ) )] public EditorTool Tool { get; private set; }

		public byte SelectedBlockId => HotbarBlockIds[CurrentHotbarIndex];

		public ActionHistory<EditorAction> UndoStack { get; private set; }
		public ActionHistory<EditorAction> RedoStack { get; private set; }

		public Dictionary<int,EditorTool> Tools { get; private set; }

		private EditorBounds EditorBounds { get; set; }
		private Nameplate Nameplate { get; set; }

		public string DisplayName => Client.Name;
		public bool IsFriendly => true;
		public Team Team => Team.Orange;

		public EditorCamera EditorCamera { get; private set; } = new();

		public EditorPlayer() : base()
		{
			HotbarBlockIds = new List<byte>();

			for ( var i = 0; i < 8; i++ )
			{
				HotbarBlockIds.Add( 0 );
			}
		}

		public EditorPlayer( IClient client ) : this()
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
			Game.AssertServer();
			UndoStack.Push( action );
			action.Perform();
		}

		public void Undo()
		{
			Game.AssertServer();

			if ( UndoStack.TryPop( out var action ) )
			{
				RedoStack.Push( action );
				action.Undo();
			}
		}

		public void Redo()
		{
			Game.AssertServer();

			if ( RedoStack.TryPop( out var action ) )
			{
				UndoStack.Push( action );
				action.Perform();
			}
		}

		public void SetActiveTool( EditorTool tool )
		{
			Game.AssertServer();

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

			base.Respawn();
		}

		public override void FrameSimulate( IClient client )
		{
			EditorCamera?.Update();
			Controller?.SetActivePlayer( this );
			Controller?.FrameSimulate();
		}

		public override void Simulate( IClient client )
		{
			SimulateAnimation();

			if ( !VoxelWorld.Current.IsValid() ) return;

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

				if ( Game.IsClient && Input.Down( "duck" ) && Input.Pressed( "back" ) )
				{
					var state = CoreWarsGame.GetStateAs<EditorState>();

					if ( !string.IsNullOrEmpty( state.CurrentFileName ) )
					{
						CoreWarsGame.SaveEditorMapCmd( state.CurrentFileName );
					}
					else
					{
						EditorSaveDialog.Open();
					}
				}

				if ( Game.IsServer && Input.Down( "duck" ) )
				{
					if ( Input.Pressed( "reload" ) )
					{
						Redo();
					}
					else if ( Input.Pressed( "menu" ) )
					{
						Undo();
					}
				}

				if ( Input.Released( "drop" ) )
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

			Controller?.SetActivePlayer( this );
			Controller?.Simulate();

			Tool?.Simulate( client );
		}

		protected override void OnDestroy()
		{
			if ( Game.IsServer || IsLocalPawn )
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
			if ( Input.Pressed( "slot1" ) ) CurrentHotbarIndex = 0;
			if ( Input.Pressed( "slot2" ) ) CurrentHotbarIndex = 1;
			if ( Input.Pressed( "slot3" ) ) CurrentHotbarIndex = 2;
			if ( Input.Pressed( "slot4" ) ) CurrentHotbarIndex = 3;
			if ( Input.Pressed( "slot5" ) ) CurrentHotbarIndex = 4;
			if ( Input.Pressed( "slot6" ) ) CurrentHotbarIndex = 5;
			if ( Input.Pressed( "slot7" ) ) CurrentHotbarIndex = 6;
			if ( Input.Pressed( "slot8" ) ) CurrentHotbarIndex = 7;
		}
	}
}
