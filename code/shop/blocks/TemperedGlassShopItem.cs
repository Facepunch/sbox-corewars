﻿using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class TemperedGlassShopItem : BaseBlockShopItem
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};
		public override int Quantity => 4;

		public override BlockType GetBlockType( CoreWarsPlayer player )
		{
			return VoxelWorld.Current.GetBlockType<TemperedGlassBlock>();
		}
	}
}
