using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class WoodenPlanksShopItem : BaseBlockShopItem
	{
		public override string Description => "A slightly stronger defensive block.";
		public override string Name => "Wooden Planks";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 34
		};
		public override int Quantity => 16;

		public override BlockType GetBlockType( Player player )
		{
			return VoxelWorld.Current.GetBlockType<WoodenPlanksBlock>();
		}
	}
}
