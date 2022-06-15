using System;
using System.Collections.Generic;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Blocks;

namespace Facepunch.CoreWars
{
	[Library]
	public class VortexBombShopItem : BaseWeaponShopItem<VortexBombItem>
	{
		public override int SortOrder => 3;

		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override int Quantity => 1;
	}
}
