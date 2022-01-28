using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	public partial class ChunkData : BaseNetworkable, INetworkSerializer
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

		public byte GetBlockTypeAtPosition( IntVector3 pos )
		{
			return BlockTypes[Chunk.GetBlockIndexAtPosition( pos )];
		}

		public byte GetBlockTypeAtIndex( int index )
		{
			return BlockTypes[index];
		}

		public void SetBlockTypeAtPosition( IntVector3 pos, byte blockType )
		{
			BlockTypes[Chunk.GetBlockIndexAtPosition( pos )] = blockType;
		}

		public void SetBlockTypeAtIndex( int index, byte blockType )
		{
			BlockTypes[index] = blockType;
		}

		public void Read( ref NetRead read )
		{
			var x = read.Read<int>();
			var y = read.Read<int>();
			var z = read.Read<int>();
			Offset = new IntVector3( x, y, z );
			BlockTypes = read.ReadUnmanagedArray( BlockTypes );
		}

		public void Write( NetWrite write )
		{
			write.Write( Offset.x );
			write.Write( Offset.y );
			write.Write( Offset.z );
			write.WriteUnmanagedArray( BlockTypes );
		}
	}
}
