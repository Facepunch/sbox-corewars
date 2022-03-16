using Sandbox;

namespace Facepunch.CoreWars
{
	[Library( "cw_voxel_world_loader" )]
	[Hammer.EntityTool( "Voxel World Loader", "Core Wars" )]
	[Hammer.EditorSprite( "editor/rts_minimap.vmat" )]
	public class VoxelWorldLoader : ModelEntity
	{
		[Property] public string FileName { get; set; }
	}
}
