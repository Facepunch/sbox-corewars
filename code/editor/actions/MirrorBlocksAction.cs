using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Facepunch.CoreWars.Editor
{
	public class MirrorBlocksAction : EditorAction
	{
		public override string Name => "Mirror Blocks";
		 
		private BlockState[] NewBlockStates { get; set; }
		private byte[] NewBlockIds { get; set; }
		private BlockState[] OldBlockStates { get; set; }
		private byte[] OldBlockIds { get; set; }
		private IntVector3 Mins { get; set; }
		private IntVector3 Maxs { get; set; }
		private List<int> EntityIds { get; set; }
		private bool ShouldMirrorEntities { get; set; }
		private bool FlipX { get; set; }
		private bool FlipY { get; set; }
		private int Width { get; set; }
		private int Height { get; set; }
		private int Depth { get; set; }

		public void Initialize( IntVector3 mins, IntVector3 maxs, bool flipX, bool flipY, bool mirrorEntities = false )
		{
			var world = VoxelWorld.Current;

			FlipX = flipX;
			FlipY = flipY;

			Mins = world.GetPositionMins( mins, maxs );
			Maxs = world.GetPositionMaxs( mins, maxs );

			Width = (Maxs.x - Mins.x) + 1;
			Height = (Maxs.y - Mins.y) + 1;
			Depth = (Maxs.z - Mins.z) + 1;

			var totalBlocks = Width * Height * Depth;

			OldBlockIds = new byte[totalBlocks];
			NewBlockIds = new byte[totalBlocks];
			OldBlockStates = new BlockState[totalBlocks];
			NewBlockStates = new BlockState[totalBlocks];

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var oldIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						OldBlockIds[oldIndex] = world.GetBlock( position );
						OldBlockStates[oldIndex] = world.GetState<BlockState>( position );
					}
				}
			}

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var localPosition = GetLocalPosition( x, y, z );
						var newIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						var oldIndex = GetArrayIndex(
							FlipX ? (Width - localPosition.x - 1) : localPosition.x,
							FlipY ? (Height - localPosition.y - 1) : localPosition.y,
							localPosition.z
						);
						NewBlockStates[newIndex] = OldBlockStates[oldIndex];
						NewBlockIds[newIndex] = OldBlockIds[oldIndex];
					}
				}
			}

			ShouldMirrorEntities = mirrorEntities;
			EntityIds = new();
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var newIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						world.SetBlockOnServer( position, NewBlockIds[newIndex] );
						world.SetState( position, NewBlockStates[newIndex] );
					}
				}
			}

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var oldIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						world.SetBlockOnServer( position, OldBlockIds[oldIndex] );
						world.SetState( position, OldBlockStates[oldIndex] );
					}
				}
			}

			/*
			foreach ( var entityId in EntityIds )
			{
				if ( FindObject<Entity>( entityId, out var entity ) )
				{
					entity.Delete();
				}
			}
			*/

			base.Undo();
		}

		private IntVector3 GetLocalPosition( int x, int y, int z )
		{
			return new IntVector3( Maxs.x - x, Maxs.y - y, Maxs.z - z );
		}

		private int GetArrayIndex( int x, int y, int z )
		{
			return x * Height * Depth + y * Depth + z;
		}

		private void MirrorEntities()
		{
			EntityIds.Clear();

			/*
			var world = VoxelWorld.Current;
			var sourceMins = world.ToSourcePosition( SourceMins );
			var sourceMaxs = world.ToSourcePosition( SourceMaxs );
			var targetMins = world.ToSourcePosition( TargetMins );
			var minX = Math.Min( sourceMins.x, sourceMaxs.x );
			var minY = Math.Min( sourceMins.y, sourceMaxs.y );
			var minZ = Math.Min( sourceMins.z, sourceMaxs.z );
			var maxX = Math.Max( sourceMins.x, sourceMaxs.x );
			var maxY = Math.Max( sourceMins.y, sourceMaxs.y );
			var maxZ = Math.Max( sourceMins.z, sourceMaxs.z );
			var entities = Entity.FindInBox( new BBox( new Vector3( minX, minY, minZ ), new Vector3( maxX, maxY, maxZ ) ) );

			foreach ( var entity in entities )
			{
				if ( entity is ISourceEntity sourceEntity )
				{
					var localPosition = entity.Position - sourceMins;
					var newPosition = targetMins + localPosition;
					var newEntity = TypeLibrary.Create<ISourceEntity>( entity.GetType() );

					newEntity.Position = newPosition;
					newEntity.Rotation = entity.Rotation;

					IVolumeEntity volumeEntity = default;
					Vector3 oldMins = default;
					Vector3 oldMaxs = default;

					if ( entity is IVolumeEntity )
					{
						volumeEntity = (entity as IVolumeEntity);

						var localMins = volumeEntity.Mins - sourceMins;
						var localMaxs = volumeEntity.Maxs - sourceMins;

						oldMins = volumeEntity.Mins;
						oldMaxs = volumeEntity.Maxs;

						volumeEntity.Mins = targetMins + localMins;
						volumeEntity.Maxs = targetMins + localMaxs;
					}

					var serialized = BinaryHelper.Serialize( w => sourceEntity.Serialize( w ) );
					BinaryHelper.Deserialize( serialized, r => newEntity.Deserialize( r ) );

					if ( volumeEntity.IsValid() )
					{
						volumeEntity.Mins = oldMins;
						volumeEntity.Maxs = oldMaxs;
					}

					EntityIds.Add( AddObject( newEntity ) );
				}
			}
			*/
		}
	}
}
