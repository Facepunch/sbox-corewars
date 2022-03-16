using Sandbox;

namespace Facepunch.CoreWars
{
	[Library( "cw_voxel_world_loader" )]
	[Hammer.EntityTool( "Voxel World Loader", "Core Wars" )]
	[Hammer.EditorSprite( "materials/editor/voxel_world_loader.vmat" )]
	public class VoxelWorldLoader : ModelEntity
	{
		[Property] public string FileName { get; set; }
	}
}
