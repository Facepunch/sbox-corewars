using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	public partial class ChunkData
	{
		public IntVector3 Offset;
		public byte[] BlockTypes;

		public ChunkData()
		{
		}

		public ChunkData( IntVector3 offset )
		{
			Offset = offset;
			BlockTypes = new byte[Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize];
		}

		public byte GetBlockTypeAtPosition( IntVector3 position )
		{
			return BlockTypes[Chunk.GetBlockIndexAtPosition( position )];
		}

		public byte GetBlockTypeAtIndex( int index )
		{
			return BlockTypes[index];
		}

		public void SetBlockTypeAtPosition( IntVector3 position, byte blockType )
		{
			BlockTypes[Chunk.GetBlockIndexAtPosition( position )] = blockType;
		}

		public void SetBlockTypeAtIndex( int index, byte blockType )
		{
			BlockTypes[index] = blockType;
		}
	}
}
