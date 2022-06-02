using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class ExplosivesShopItem : BaseBlockShopItem
	{
		public override string Description => "A high strength timed explosives block.";
		public override string Name => "Explosives";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override int Quantity => 1;

		public override BlockType GetBlockType( Player player )
		{
			return VoxelWorld.Current.GetBlockType<ExplosivesBlock>();
		}
	}
}
