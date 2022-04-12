using Sandbox;
using Hammer;
using System.ComponentModel.DataAnnotations;

namespace Facepunch.CoreWars
{
	[Library( "cw_voxel_world_loader" )]
	[Display( Name = "Voxel World Loader", GroupName = "Core Wars" ) ]
	[Hammer.EditorSprite( "materials/editor/voxel_world_loader.vmat" )]
	public class VoxelWorldLoader : ModelEntity
	{
		[Property] public string FileName { get; set; }
	}
}
