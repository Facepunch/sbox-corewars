using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkLightMap
	{
		public Texture Texture { get; private set; }
		public byte[] Data;
		public Chunk Chunk { get; private set; }
		public int ChunkSize;

		public ChunkLightMap( Chunk chunk )
		{
			ChunkSize = Chunk.ChunkSize;
			Data = new byte[ChunkSize * ChunkSize * ChunkSize * 4];
			Texture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.RGBA8888 )
				.WithData( Data )
				.Finish();

			Chunk = chunk;
		}

		public int ToIndex( IntVector3 position, int component )
		{
			return (((position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x) * 4) + component;
		}

		public byte GetSunlight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return Data[index];
		}

		public void SetSunlight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			Data[index] = value;
		}

		public byte GetTorchlight( IntVector3 position )
		{
			var index = ToIndex( position, 1 );
			return Data[index];
		}

		public void SetTorchlight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 1 );
			Data[index] = value;
		}

		public void Update()
		{
			Texture.Update( Data );
		}
	}
}
