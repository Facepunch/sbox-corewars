using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	public static class ClientExtensions
	{
		public static ChunkViewer GetChunkViewer( this Client client )
		{
			return Map.Current?.GetViewer( client );
		}
	}
}
