using Sandbox;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars.Voxel
{
	public partial class ChunkViewer : EntityComponent
	{
		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		[ClientRpc]
		public static void UnloadChunkForClient( int x, int y, int z )
		{
			var client = Local.Client;

			if ( client.Components.TryGet<ChunkViewer>( out var viewer ) )
			{
				var chunk = Map.Current.GetChunk( new IntVector3( x, y, z ) );

				if ( chunk.IsValid() )
				{
					viewer.RemoveLoadedChunk( chunk );
					Map.Current.RemoveChunk( chunk );
				}
			}
		}

		public float UnloadChunkDistance { get; set; } = 16f;
		public float LoadChunkDistance { get; set; } = 8f;
		public HashSet<IntVector3> LoadedChunks { get; private set; }
		public HashSet<Chunk> ChunksToRemove { get; private set; }
		public HashSet<Chunk> ChunksToSend { get; private set; }
		public Queue<Chunk> ChunkSendQueue { get; private set; }

		public bool IsChunkLoaded( Chunk chunk )
		{
			return LoadedChunks.Contains( chunk.Offset );
		}

		public void AddLoadedChunk( Chunk chunk )
		{
			if ( IsServer )
			{
				ChunkSendQueue.Enqueue( chunk );
				ChunksToRemove.Remove( chunk );
			}
			else
			{
				LoadedChunks.Add( chunk.Offset );
			}
		}

		public void RemoveLoadedChunk( Chunk chunk )
		{
			if ( IsServer )
			{
				ChunksToRemove.Add( chunk );
			}
			else
			{
				LoadedChunks.Remove( chunk.Offset );
			}
		}

		public void ClearLoadedChunks()
		{
			LoadedChunks.Clear();
		}

		public void Update()
		{
			if ( Entity is not Client client ) return;

			var pawn = client.Pawn;
			if ( !pawn.IsValid() ) return;

			var position = pawn.Position;

			foreach ( var offset in LoadedChunks )
			{
				var chunk = Map.Current.GetChunk( offset );

				if ( chunk.IsValid() )
				{
					var chunkPositionCenter = chunk.Offset + new IntVector3( Chunk.ChunkSize / 2 );
					var chunkPositionSource = Map.ToSourcePosition( chunkPositionCenter );

					if ( position.Distance( chunkPositionSource ) >= Chunk.ChunkSize * Chunk.VoxelSize * UnloadChunkDistance )
					{
						RemoveLoadedChunk( chunk );
					}
				}
			}

			var voxelPosition = Map.ToVoxelPosition( position );
			var currentChunk = Map.Current.GetChunk( voxelPosition );

			if ( currentChunk.IsValid() )
			{
				AddLoadedChunk( currentChunk );
			}

			while ( ChunkSendQueue.Count > 0 )
			{
				var chunk = ChunkSendQueue.Dequeue();
				var chunkPositionCenter = chunk.Offset + new IntVector3( Chunk.ChunkSize / 2 );
				var chunkPositionSource = Map.ToSourcePosition( chunkPositionCenter );

				if ( !ChunksToSend.Contains( chunk ) && position.Distance( chunkPositionSource ) <= Chunk.ChunkSize * Chunk.VoxelSize * LoadChunkDistance )
				{
					ChunksToSend.Add( chunk );

					foreach ( var neighbour in chunk.GetNeighbours() )
					{
						ChunkSendQueue.Enqueue( neighbour );
					}
				}
			}

			if ( ChunksToSend.Count > 0 )
			{
				using ( var stream = new MemoryStream() )
				{
					using ( var writer = new BinaryWriter( stream ) )
					{
						var unloadedChunks = ChunksToSend.Where( c => !IsChunkLoaded( c ) && c.HasDoneFirstFullUpdate );
						writer.Write( unloadedChunks.Count() );

						foreach ( var chunk in unloadedChunks )
						{
							writer.Write( chunk.Offset.x );
							writer.Write( chunk.Offset.y );
							writer.Write( chunk.Offset.z );
							writer.Write( chunk.Blocks );

							//chunk.LightMap.Serialize( writer );
							chunk.SerializeData( writer );

							LoadedChunks.Add( chunk.Offset );
						}

						var compressed = CompressionHelper.Compress( stream.ToArray() );
						Map.ReceiveChunks( To.Single( client ), compressed );
					}
				}

				ChunksToSend.Clear();
			}

			if ( ChunksToRemove.Count > 0 )
			{
				foreach ( var chunk in ChunksToRemove )
				{
					UnloadChunkForClient( To.Single( client ), chunk.Offset.x, chunk.Offset.y, chunk.Offset.z );
					LoadedChunks.Remove( chunk.Offset );
				}

				ChunksToRemove.Clear();
			}
		}

		protected override void OnActivate()
		{
			ChunksToRemove = new();
			ChunkSendQueue = new();
			ChunksToSend = new();
			LoadedChunks = new();

			base.OnActivate();
		}
	}
}
