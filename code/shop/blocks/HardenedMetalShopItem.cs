using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class HardenedMetalShopItem : BaseBlockShopItem
	{
		public override string Description => "A medium-strength defensive block.";
		public override string Name => "Hardened Metal";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override int Quantity => 16;

		public override BlockType GetBlockType( Player player )
		{
			return VoxelWorld.Current.GetBlockType<HardenedMetalBlock>();
		}
	}
}
