using Sandbox;
using System.Runtime.InteropServices;

namespace Facepunch.CoreWars.Voxel
{
	[StructLayout( LayoutKind.Sequential )]
	public struct BlockVertex
	{
		private readonly uint Data;

		public BlockVertex( uint x, uint y, uint z, uint faceData )
		{
			Data = (faceData | (x & 63) | (y & 63) << 6 | (z & 63) << 12);
		}

		public static readonly VertexAttribute[] Layout =
		{
			new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.UInt32, 1, 10 )
		};
	}
}
