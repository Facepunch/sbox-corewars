using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	public enum Team
	{
		None,
		Blue,
		Red,
		Orange,
		Green,
		Purple,
		Pink,
		Cyan,
		Yellow
	}

	public static class TeamExtensions
	{
		public static BlockType GetPlasticBlock( this Team team )
		{
			var world = VoxelWorld.Current;
			if ( !world.IsValid() ) return null;

			var blockId = team switch
			{
				Team.Blue => world.FindBlockId<BluePlasticBlock>(),
				Team.Red => world.FindBlockId<RedPlasticBlock>(),
				Team.Orange => world.FindBlockId<OrangePlasticBlock>(),
				Team.Green => world.FindBlockId<GreenPlasticBlock>(),
				Team.Purple => world.FindBlockId<PurplePlasticBlock>(),
				Team.Pink => world.FindBlockId<PinkPlasticBlock>(),
				Team.Cyan => world.FindBlockId<CyanPlasticBlock>(),
				Team.Yellow => world.FindBlockId<YellowPlasticBlock>(),
				_ => throw new System.NotImplementedException()
			};

			return world.GetBlockType( blockId );
		}

		public static BlockItem CreatePlasticBlockItem( this Team team )
		{
			var block = team.GetPlasticBlock();
			if ( block == null ) return null;

			var item = InventorySystem.CreateItem<BlockItem>();
			item.BlockId = block.BlockId;

			return item;
		}
	}
}
