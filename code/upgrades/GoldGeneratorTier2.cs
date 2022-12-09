using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class GoldGeneratorTier2 : BaseTeamUpgrade
	{
		public override string Name => "100% Gold Generator Speed";
		public override Color Color => UI.ColorPalette.Resources;
		public override string Description => "Your team's generator spawns Gold 100% faster.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 8
		};
		public override string Group => "gold";
		public override int Tier => 2;

		public override string GetIcon( CoreWarsPlayer player )
		{
			return "textures/upgrades/gold_2.png";
		}
	}
}
