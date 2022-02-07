using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkDataMap
	{
		public HashSet<IntVector3> DirtyPositions { get; private set; }
		public Texture Texture { get; private set; }
		public Chunk Chunk { get; private set; }
		public Map Map { get; private set; }
		public byte[] Data;
		public int ChunkSize;

		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		private bool IsDirty { get; set; }

		public ChunkDataMap( Chunk chunk, Map map )
		{
			DirtyPositions = new();
			ChunkSize = Chunk.ChunkSize;
			Chunk = chunk;
			Map = map;

			Data = new byte[ChunkSize * ChunkSize * ChunkSize];
			Texture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( Data )
				.Finish();

			Array.Fill<byte>( Data, 100 );
		}

		public void Copy( byte[] data )
		{
			IsDirty = true;
			Data = data;
		}

		public int ToIndex( IntVector3 position )
		{
			return (position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x;
		}

		public void Update()
		{
			if ( !IsDirty ) return;

			IsDirty = false;

			if ( IsServer )
			{
				foreach ( var position in DirtyPositions )
				{
					var index = ToIndex( position );
					Map.ReceiveDataUpdate( To.Everyone, Chunk.ToMapPosition( position ), Data[index] );
				}

				DirtyPositions.Clear();
			}
			else
			{
				Texture.Update( Data );
			}
		}

		public byte GetHealth( IntVector3 position )
		{
			var index = ToIndex( position );
			return Data[index];
		}

		public bool SetHealth( IntVector3 position, byte value )
		{
			var index = ToIndex( position );
			if ( GetHealth( position ) == value ) return false;

			if ( IsServer && !DirtyPositions.Contains( position ) )
			{
				DirtyPositions.Add( position );
			}

			IsDirty = true;
			Data[index] = value;

			return true;
		}
	}
}
