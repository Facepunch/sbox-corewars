using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	[Library( "block" ), AutoGenerate]
	public class BlockAsset : Asset
	{
		[Property] public string FriendlyName { get; set; }
		[Property] public byte TextureId { get; set; }
	}
}
