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
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 24
		};
		public override int Quantity => 8;

		public override BlockType GetBlockType( CoreWarsPlayer player )
		{
			return VoxelWorld.Current.GetBlockType<WoodenPlanksBlock>();
		}
	}
}
