using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PlainsBiome : Biome
	{
		public override string Name => "Plains";

		public override void Initialize()
		{
			TopBlockId = Map.FindBlockId<GrassBlock>();
			BeachBlockId = Map.FindBlockId<SandBlock>();
			GroundBlockId = Map.FindBlockId<GrassBlock>();
			TreeLogBlockId = Map.FindBlockId<WoodBlock>();
			TreeLeafBlockId = Map.FindBlockId<LeafBlock>();
			LiquidBlockId = Map.FindBlockId<WaterBlock>();
			UndergroundBlockId = Map.FindBlockId<StoneBlock>();

			SetWeighting( 0.5f );
		}
	}
}
