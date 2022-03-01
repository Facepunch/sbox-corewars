using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Editor
{
	public class EditorState : BaseState
	{
		public virtual async Task LoadInitialChunks( VoxelWorld world )
		{
			var fileName = "editor.voxels";

			if ( !FileSystem.Data.FileExists( fileName ) )
			{
				return;
			}

			var bytes = await FileSystem.Data.ReadAllBytesAsync( fileName );
			var blockIdRemap = new Dictionary<byte, byte>();

			try
			{
				//var decompressed = CompressionHelper.Decompress( bytes );

				using ( var stream = new MemoryStream( bytes ) )
				{
					using ( var reader = new BinaryReader( stream ) )
					{
						var voxelSize = reader.ReadInt32();
						var maxSizeX = reader.ReadInt32();
						var maxSizeY = reader.ReadInt32();
						var maxSizeZ = reader.ReadInt32();
						var chunkSizeX = reader.ReadInt32();
						var chunkSizeY = reader.ReadInt32();
						var chunkSizeZ = reader.ReadInt32();

						world.SetVoxelSize( voxelSize );
						world.SetMaxSize( maxSizeX, maxSizeY, maxSizeZ );
						world.SetChunkSize( chunkSizeX, chunkSizeY, chunkSizeZ );

						var blockCount = reader.ReadInt32();

						for ( var i = 0; i < blockCount; i++ )
						{
							var blockId = reader.ReadByte();
							var blockType = reader.ReadString();

							if ( !world.BlockTypes.TryGetValue( blockType, out var realBlockId ) )
								throw new Exception( $"Unable to locate a block id for {blockType}!" );

							blockIdRemap[blockId] = realBlockId;
						}

						var chunkCount = reader.ReadInt32();

						for ( var i = 0; i < chunkCount; i++ )
						{
							var chunkX = reader.ReadInt32();
							var chunkY = reader.ReadInt32();
							var chunkZ = reader.ReadInt32();
							var chunk = world.GetOrCreateChunk( chunkX, chunkY, chunkZ );

							chunk.HasOnlyAirBlocks = reader.ReadBoolean();

							if ( !chunk.HasOnlyAirBlocks )
								chunk.Blocks = reader.ReadBytes( world.ChunkSize.x * world.ChunkSize.y * world.ChunkSize.z );

							for ( var j = 0; j < chunk.Blocks.Length; j++ )
							{
								var currentBlockId = chunk.Blocks[j];

								if ( blockIdRemap.TryGetValue( currentBlockId, out var remappedBlockId ) )
								{
									chunk.Blocks[j] = remappedBlockId;
								}
							}

							chunk.DeserializeData( reader );

							await GameTask.Delay( 5 );
						}
					}
				}
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
		}

		public virtual void SaveChunksToDisk( VoxelWorld world )
		{
			var fileName = "editor.voxels";

			try
			{
				using ( var stream = FileSystem.Data.OpenWrite( fileName, FileMode.Create ) )
				{
					using ( var writer = new BinaryWriter( stream ) )
					{
						writer.Write( world.VoxelSize );
						writer.Write( world.MaxSize.x );
						writer.Write( world.MaxSize.y );
						writer.Write( world.MaxSize.z );
						writer.Write( world.ChunkSize.x );
						writer.Write( world.ChunkSize.y );
						writer.Write( world.ChunkSize.z );

						writer.Write( world.BlockTypes.Count );

						foreach ( var kv in world.BlockTypes )
						{
							writer.Write( kv.Value );
							writer.Write( kv.Key );
						}

						writer.Write( world.Chunks.Count );

						foreach ( var kv in world.Chunks )
						{
							var chunk = kv.Value;

							writer.Write( chunk.Offset.x );
							writer.Write( chunk.Offset.y );
							writer.Write( chunk.Offset.z );
							writer.Write( chunk.HasOnlyAirBlocks );

							if ( !chunk.HasOnlyAirBlocks )
								writer.Write( chunk.Blocks );

							chunk.SerializeData( writer );
						}
					}
				}

				Log.Info( $"Saved chunks to disk ({fileName})..." );
			}
			catch ( Exception e )
			{
				Log.Error( e );
			}
		}

		public override void OnEnter()
		{
			if ( Host.IsServer )
			{
				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.Respawn();
				}
			}
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			player.Respawn();
		}
	}
}
