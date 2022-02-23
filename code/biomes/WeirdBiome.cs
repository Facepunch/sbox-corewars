using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class WeirdBiome : Biome
	{
		public override string Name => "Weird";

		public override void Initialize()
		{
			TopBlockId = VoxelWorld.FindBlockId<SandBlock>();
			BeachBlockId = VoxelWorld.FindBlockId<SandBlock>();
			GroundBlockId = VoxelWorld.FindBlockId<SandBlock>();
			TreeLogBlockId = VoxelWorld.FindBlockId<StoneBlock>();
			TreeLeafBlockId = VoxelWorld.FindBlockId<GrassBlock>();
			LiquidBlockId = VoxelWorld.FindBlockId<WaterBlock>();
			UndergroundBlockId = VoxelWorld.FindBlockId<StoneBlock>();

			SetWeighting( 0.2f );
		}
	}
}
