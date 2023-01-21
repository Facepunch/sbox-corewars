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
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override int Quantity => 8;

		public override BlockType GetBlockType( CoreWarsPlayer player )
		{
			return VoxelWorld.Current.GetBlockType<HardenedMetalBlock>();
		}
	}
}
