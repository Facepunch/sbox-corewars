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
	public class PlasticBlockShopItem : BaseBlockShopItem
	{
		public override string Description => "The cheapest but weakest block.";
		public override string Name => "Plastic Block";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 8
		};
		public override int Quantity => 8;

		public override BlockType GetBlockType( Player player )
		{
			return player.Team.GetPlasticBlock();
		}
	}
}
