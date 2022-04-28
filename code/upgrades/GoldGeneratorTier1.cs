using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class GoldGeneratorTier1 : BaseTeamUpgrade
	{
		public override string Name => "Gold Generator";
		public override string Description => "Your team's generator occasionally spawns Gold.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 4
		};
		public override string Group => "gold";
		public override int Tier => 1;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/gold_1.png";
		}
	}
}
