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
					viewer.RemoveLoadedChunk( chunk.Offset );
					Map.Current.RemoveChunk( chunk );
				}
			}
		}

		public float UnloadChunkDistance { get; set; } = 16f;
		public float LoadChunkDistance { get; set; } = 4f;
		public HashSet<IntVector3> LoadedChunks { get; private set; }
		public HashSet<IntVector3> ChunksToRemove { get; private set; }
		public HashSet<Chunk> ChunksToSend { get; private set; }
		public Queue<IntVector3> ChunkSendQueue { get; private set; }

		public bool IsChunkLoaded( IntVector3 offset )
		{
			return LoadedChunks.Contains( offset );
		}

		public void AddLoadedChunk( IntVector3 offset )
		{
			if ( IsServer )
			{
				ChunkSendQueue.Enqueue( offset );
				ChunksToRemove.Remove( offset );
			}
			else
			{
				LoadedChunks.Add( offset );
			}
		}

		public void RemoveLoadedChunk( IntVector3 offset )
		{
			if ( IsServer )
			{
				ChunksToRemove.Add( offset );
			}
			else
			{
				LoadedChunks.Remove( offset );
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
						RemoveLoadedChunk( chunk.Offset );
					}
				}
			}

			var voxelPosition = Map.ToVoxelPosition( position );
			var currentChunkOffset = Map.Current.ToChunkOffset( voxelPosition );

			if ( Map.Current.IsInBounds( currentChunkOffset ) )
			{
				AddLoadedChunk( currentChunkOffset );
			}

			while ( ChunkSendQueue.Count > 0 )
			{
				var offset = ChunkSendQueue.Dequeue();
				var chunkPositionCenter = offset + new IntVector3( Chunk.ChunkSize / 2 );
				var chunkPositionSource = Map.ToSourcePosition( chunkPositionCenter );

				if ( position.Distance( chunkPositionSource ) <= Chunk.ChunkSize * Chunk.VoxelSize * LoadChunkDistance )
				{
					var chunk = Map.Current.GetOrCreateChunk( offset );
					if ( !chunk.IsValid() ) continue;

					if ( !chunk.Initialized )
					{
						chunk.Initialize();
					}

					if ( !ChunksToSend.Contains( chunk ) )
					{
						ChunksToSend.Add( chunk );

						foreach ( var neighbour in chunk.GetNeighbourOffsets() )
						{
							if ( Map.Current.IsInBounds( neighbour ) )
								ChunkSendQueue.Enqueue( neighbour );
						}
					}
				}
			}

			if ( ChunksToSend.Count > 0 )
			{
				using ( var stream = new MemoryStream() )
				{
					using ( var writer = new BinaryWriter( stream ) )
					{
						var unloadedChunks = ChunksToSend.Where( c => !IsChunkLoaded( c.Offset ) && c.HasDoneFirstFullUpdate );
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
					UnloadChunkForClient( To.Single( client ), chunk.x, chunk.y, chunk.z );
					LoadedChunks.Remove( chunk );
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
