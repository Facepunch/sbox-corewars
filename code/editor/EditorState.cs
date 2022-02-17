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
		public virtual async Task LoadInitialChunks( Map map )
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

						map.SetVoxelSize( voxelSize );
						map.SetMaxSize( maxSizeX, maxSizeY, maxSizeZ );
						map.SetChunkSize( chunkSizeX, chunkSizeY, chunkSizeZ );

						var blockCount = reader.ReadInt32();

						for ( var i = 0; i < blockCount; i++ )
						{
							var blockId = reader.ReadByte();
							var blockType = reader.ReadString();

							if ( !map.BlockTypes.TryGetValue( blockType, out var realBlockId ) )
								throw new Exception( $"Unable to locate a block id for {blockType}!" );

							blockIdRemap[blockId] = realBlockId;
						}

						var chunkCount = reader.ReadInt32();

						for ( var i = 0; i < chunkCount; i++ )
						{
							var chunkX = reader.ReadInt32();
							var chunkY = reader.ReadInt32();
							var chunkZ = reader.ReadInt32();
							var chunk = map.GetOrCreateChunk( chunkX, chunkY, chunkZ );

							chunk.HasOnlyAirBlocks = reader.ReadBoolean();

							if ( !chunk.HasOnlyAirBlocks )
								chunk.Blocks = reader.ReadBytes( map.ChunkSize.x * map.ChunkSize.y * map.ChunkSize.z );

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

		public virtual void SaveChunksToDisk( Map map )
		{
			var fileName = "editor.voxels";

			try
			{
				using ( var stream = FileSystem.Data.OpenWrite( fileName, FileMode.Create ) )
				{
					using ( var writer = new BinaryWriter( stream ) )
					{
						writer.Write( map.VoxelSize );
						writer.Write( map.MaxSize.x );
						writer.Write( map.MaxSize.y );
						writer.Write( map.MaxSize.z );
						writer.Write( map.ChunkSize.x );
						writer.Write( map.ChunkSize.y );
						writer.Write( map.ChunkSize.z );

						writer.Write( map.BlockTypes.Count );

						foreach ( var kv in map.BlockTypes )
						{
							writer.Write( kv.Value );
							writer.Write( kv.Key );
						}

						writer.Write( map.Chunks.Count );

						foreach ( var kv in map.Chunks )
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
