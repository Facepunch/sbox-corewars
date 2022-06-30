using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Facepunch.Voxels;

namespace Facepunch.CoreWars
{
	[Library]
	public class FungusBlockShopItem : BaseBlockShopItem
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 8
		};
		public override int Quantity => 8;

		public override BlockType GetBlockType( Player player )
		{
			return player.Team.GetFungusBlock();
		}
	}
}
