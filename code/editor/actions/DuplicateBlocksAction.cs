using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Facepunch.CoreWars.Editor
{
	public class DuplicateBlocksAction : EditorAction
	{
		public override string Name => "Duplicate Blocks";
		 
		private IEnumerable<IntVector3> SourcePositions { get; set; }
		private BlockState[] NewBlockStates { get; set; }
		private byte[] NewBlockIds { get; set; }
		private BlockState[] OldBlockStates { get; set; }
		private byte[] OldBlockIds { get; set; }
		private IntVector3[] TargetPositions { get; set; }
		private IntVector3 SourceMins { get; set; }
		private IntVector3 SourceMaxs { get; set; }
		private IntVector3 TargetMins { get; set; }
		private List<int> EntityIds { get; set; }
		private bool ShouldCopyEntities { get; set; }

		public void Initialize( IntVector3 sourceMins, IntVector3 sourceMaxs, IntVector3 targetMins, bool copyEntities = false, float rotation = 0f )
		{
			var world = VoxelWorld.Current;

			SourceMins = sourceMins;
			SourceMaxs = sourceMaxs;
			TargetMins = targetMins;
			SourcePositions = world.GetPositionsInBox( SourceMins, SourceMaxs );

			var targetSourceMins = world.ToSourcePositionCenter( TargetMins );
			var targetSourceMaxs = world.ToSourcePositionCenter( TargetMins + (SourceMaxs - SourceMins) );
			var totalBlocks = SourcePositions.Count();
			var currentIndex = 0;

			ShouldCopyEntities = copyEntities;
			TargetPositions = new IntVector3[totalBlocks];
			OldBlockStates = new BlockState[totalBlocks];
			OldBlockIds = new byte[totalBlocks];
			NewBlockStates = new BlockState[totalBlocks];
			NewBlockIds = new byte[totalBlocks];
			EntityIds = new();

			var targetSourceCenter = targetSourceMins + (targetSourceMaxs - targetSourceMins) * 0.5f;

			foreach ( var position in SourcePositions )
			{
				var localPosition = position - SourceMins;
				var newVoxelPosition = TargetMins + localPosition;
				var rotatedSourcePosition = world.ToSourcePositionCenter( newVoxelPosition );

				rotatedSourcePosition = rotatedSourcePosition.RotateAboutPoint( targetSourceCenter, Vector3.Up, rotation );
				newVoxelPosition = world.ToVoxelPosition( rotatedSourcePosition );

				TargetPositions[currentIndex] = newVoxelPosition;

				OldBlockStates[currentIndex] = world.GetState<BlockState>( newVoxelPosition );
				OldBlockIds[currentIndex] = world.GetBlock( newVoxelPosition );

				var state = world.GetState<BlockState>( position );

				if ( state.IsValid() )
				{
					NewBlockStates[currentIndex] = state.Copy();
				}

				NewBlockIds[currentIndex] = world.GetBlock( position );

				currentIndex++;
			}
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;

			for ( var i = 0; i < TargetPositions.Length; i++ )
			{
				world.SetBlockOnServer( TargetPositions[i], NewBlockIds[i] );
				world.SetState( TargetPositions[i], NewBlockStates[i] );
			}

			if ( ShouldCopyEntities )
			{
				CopyAndPasteEntities();
			}

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;

			for ( var i = 0; i < TargetPositions.Length; i++ )
			{
				world.SetBlockOnServer( TargetPositions[i], OldBlockIds[i] );
				world.SetState( TargetPositions[i], OldBlockStates[i] );
			}

			foreach ( var entityId in EntityIds )
			{
				if ( FindObject<Entity>( entityId, out var entity ) )
				{
					entity.Delete();
				}
			}

			base.Undo();
		}

		private void CopyAndPasteEntities()
		{
			EntityIds.Clear();

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
		}
	}
}
