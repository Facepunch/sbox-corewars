using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public abstract partial class EditorTool : BaseNetworkable, IValid
	{
		public virtual string Name => "Tool";
		public virtual string SecondaryMode => string.Empty;

		[Net] public EditorPlayer Player { get; set; }

		public bool IsValid => true;

		public virtual void Simulate( IClient client )
		{
			if ( Input.Pressed( "attack1" ) )
			{
				OnPrimary( client );
			}

			if ( Input.Pressed( "attack2" ) )
			{
				OnSecondary( client );
			}

			var world = VoxelWorld.Current;

			if ( world.IsValid() )
			{
				DebugOverlay.Axis( world.ToSourcePositionCenter( world.MaxSize / 2 ), Rotation.Identity, 100, 0f, false );
			}
		}

		public virtual void OnSelected()
		{
			var display = EditorToolDisplay.Current;
			display?.AddHotkey( "use", "Aim Voxel Align" );
			display?.AddHotkey( "flashlight", "Double Range" );
		}

		public virtual void OnDeselected()
		{
			if ( Game.IsClient )
			{
				var display = EditorToolDisplay.Current;
				display?.ClearHotkeys();
			}
		}

		protected EditorPlayer GetSimulatingPlayer()
		{
			if ( Prediction.CurrentHost.IsValid() )
				return Prediction.CurrentHost.Pawn as EditorPlayer;
			else
				return Game.LocalPawn as EditorPlayer;
		}

		protected BlockFace GetTargetBlockFace( float range )
		{
			var player = GetSimulatingPlayer();
			var distance = VoxelWorld.Current.VoxelSize * range;
			return VoxelWorld.Current.Trace( player.EyePosition * (1.0f / VoxelWorld.Current.VoxelSize), player.EyeRotation.Forward, distance, out var _, out _ );
		}

		protected IntVector3 GetAimVoxelPosition( float range )
		{
			if ( Input.Down( "flashlight" ) ) range *= 2f;

			var player = GetSimulatingPlayer();
			var distance = VoxelWorld.Current.VoxelSize * range;
			var aimVoxelPosition = VoxelWorld.Current.ToVoxelPosition( player.EyePosition + player.EyeRotation.Forward * distance );

			if ( Input.Down( "use" ) )
			{
				var face = VoxelWorld.Current.Trace( player.EyePosition * (1.0f / VoxelWorld.Current.VoxelSize), player.EyeRotation.Forward, distance, out var endPosition, out _ );

				if ( face != BlockFace.Invalid && VoxelWorld.Current.GetBlock( endPosition ) != 0 )
				{
					var oppositePosition = VoxelWorld.GetAdjacentPosition( endPosition, (int)face );
					aimVoxelPosition = oppositePosition;
				}
			}

			return aimVoxelPosition;
		}

		protected virtual void OnPrimary( IClient client )
		{

		}

		protected virtual void OnSecondary( IClient client )
		{

		}
	}
}
