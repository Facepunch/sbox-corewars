using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PlainsBiome : Biome
	{
		public override string Name => "Plains";

		public override void Initialize()
		{
			TopBlockId = VoxelWorld.FindBlockId<GrassBlock>();
			BeachBlockId = VoxelWorld.FindBlockId<SandBlock>();
			GroundBlockId = VoxelWorld.FindBlockId<GrassBlock>();
			TreeLogBlockId = VoxelWorld.FindBlockId<WoodBlock>();
			TreeLeafBlockId = VoxelWorld.FindBlockId<LeafBlock>();
			LiquidBlockId = VoxelWorld.FindBlockId<WaterBlock>();
			UndergroundBlockId = VoxelWorld.FindBlockId<StoneBlock>();

			SetWeighting( 0.5f );
		}
	}
}
