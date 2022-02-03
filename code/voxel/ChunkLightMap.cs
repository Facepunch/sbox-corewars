using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkLightMap
	{
		public Texture Texture { get; private set; }
		// We'll nuke this later
		public Texture Texture2 { get; private set; }
		public byte[] Data;
		public byte[] Data2;
		public Chunk Chunk { get; private set; }
		public int ChunkSize;

		public ChunkLightMap( Chunk chunk )
		{
			ChunkSize = Chunk.ChunkSize;
			Data = new byte[ChunkSize * ChunkSize * ChunkSize];
			Data2 = new byte[ChunkSize * ChunkSize * ChunkSize];
			
			Texture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( Data )
				.Finish();

			Texture2 = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( Data )
				.Finish();


			Chunk = chunk;
		}

		public int ToIndex( IntVector3 position, int component )
		{
			return (position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x;
		}

		public byte GetSunlight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return Data2[index];
		}

		public void SetSunlight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			Data2[index] = value;
		}

		public byte GetTorchlight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return Data[index];
		}

		public void SetTorchlight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			Data[index] = value;
		}

		public void Update()
		{
			Texture.Update( Data );
			Texture2.Update( Data2 );
		}
	}
}
