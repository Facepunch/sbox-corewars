using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class TemperedGlassShopItem : BaseBlockShopItem
	{
		public override string Description => "An easily broken but blastproof defensive block.";
		public override string Name => "Tempered Glass";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 18
		};
		public override int Quantity => 4;

		public override BlockType GetBlockType( Player player )
		{
			return VoxelWorld.Current.GetBlockType<TemperedGlassBlock>();
		}
	}
}
