﻿using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public abstract partial class EditorTool : BaseNetworkable, IValid
	{
		public virtual string Name => "Tool";
		public virtual string SecondaryMode => string.Empty;

		[Net] public EditorPlayer Player { get; set; }

		public bool IsClient => Host.IsClient;
		public bool IsServer => Host.IsServer;
		public bool IsValid => true;

		public virtual void Simulate( Client client )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				OnPrimary( client );
			}

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
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
			display?.AddHotkey( InputButton.Use, "Aim Voxel Align" );
			display?.AddHotkey( InputButton.Flashlight, "Double Range" );
		}

		public virtual void OnDeselected()
		{
			if ( IsClient )
			{
				var display = EditorToolDisplay.Current;
				display?.ClearHotkeys();
			}
		}

		protected IntVector3 GetAimVoxelPosition( float range )
		{
			if ( Input.Down( InputButton.Flashlight ) ) range *= 2f;

			var distance = VoxelWorld.Current.VoxelSize * range;
			var aimVoxelPosition = VoxelWorld.Current.ToVoxelPosition( Input.Position + Input.Rotation.Forward * distance );

			if ( Input.Down( InputButton.Use ) )
			{
				var face = VoxelWorld.Current.Trace( Input.Position * (1.0f / VoxelWorld.Current.VoxelSize), Input.Rotation.Forward, distance, out var endPosition, out _ );

				if ( face != BlockFace.Invalid && VoxelWorld.Current.GetBlock( endPosition ) != 0 )
				{
					var oppositePosition = VoxelWorld.GetAdjacentPosition( endPosition, (int)face );
					aimVoxelPosition = oppositePosition;
				}
			}

			return aimVoxelPosition;
		}

		protected virtual void OnPrimary( Client client )
		{

		}

		protected virtual void OnSecondary( Client client )
		{

		}
	}
}
