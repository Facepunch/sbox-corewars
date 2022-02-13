using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class WeirdBiome : Biome
	{
		public override string Name => "Weird";

		public override void Initialize()
		{
			TopBlockId = Map.FindBlockId<SandBlock>();
			BeachBlockId = Map.FindBlockId<SandBlock>();
			GroundBlockId = Map.FindBlockId<SandBlock>();
			TreeLogBlockId = Map.FindBlockId<StoneBlock>();
			TreeLeafBlockId = Map.FindBlockId<GrassBlock>();
			LiquidBlockId = Map.FindBlockId<WaterBlock>();
			UndergroundBlockId = Map.FindBlockId<StoneBlock>();

			SetWeighting( 0.2f );
		}
	}
}
