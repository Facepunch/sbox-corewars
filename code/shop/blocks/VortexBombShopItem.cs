using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class VortexBombShopItem : BaseBlockShopItem
	{
		public override string Description => "A high strength timed explosives block.";
		public override string Name => "Vortex Bomb";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override int Quantity => 1;

		public override BlockType GetBlockType( Player player )
		{
			return VoxelWorld.Current.GetBlockType<VortexBombBlock>();
		}
	}
}
