using Sandbox;
using System.Runtime.InteropServices;

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
			Data = new byte[ChunkSize * ChunkSize * ChunkSize];
			Texture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( Data )
				.Finish();

			Chunk = chunk;
		}

		public int ToIndex( IntVector3 position )
		{
			return (position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x;
		}

		public byte GetSunlight( IntVector3 position )
		{
			var index = ToIndex( position );
			return (byte)(Data[index] & 0xf);
		}

		public void SetSunlight( IntVector3 position, byte value )
		{
			var index = ToIndex( position );
			Data[index] = (byte)((Data[index] & 0xf0) | (value & 0xf));
		}

		public byte GetTorchlight( IntVector3 position )
		{
			var index = ToIndex( position );
			return (byte)((Data[index] >> 4) & 0xf);
		}

		public void SetTorchlight( IntVector3 position, byte value )
		{
			var index = ToIndex( position );
			Data[index] = (byte)((Data[index] & 0xf) | ((value & 0xf) << 4));
		}

		public void Update()
		{
			Texture.Update( Data );
		}
	}
}
