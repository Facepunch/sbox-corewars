using Sandbox;
using SandboxEditor;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.CoreWars
{
	[Library( "cw_voxel_world_loader" )]
	[Title( "Voxel World Loader" ) ]
	[EditorSprite( "materials/editor/voxel_world_loader.vmat" )]
	[HammerEntity]
	public class VoxelWorldLoader : ModelEntity
	{
		[Property] public string FileName { get; set; }
	}
}
