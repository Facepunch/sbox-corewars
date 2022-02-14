using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public partial class ChunkViewer : EntityComponent
	{
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
					chunk.Destroy();
				}
			}
		}

		public HashSet<IntVector3> LoadedChunks { get; private set; }

		public bool IsChunkLoaded( Chunk chunk )
		{
			return LoadedChunks.Contains( chunk.Offset );
		}

		public void AddLoadedChunk( Chunk chunk )
		{
			LoadedChunks.Add( chunk.Offset );
		}

		public void RemoveLoadedChunk( Chunk chunk )
		{
			LoadedChunks.Remove( chunk.Offset );
		}

		public void ClearLoadedChunks()
		{
			LoadedChunks.Clear();
		}

		protected override void OnActivate()
		{
			LoadedChunks = new();

			base.OnActivate();
		}
	}
}
