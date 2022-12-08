using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class TeamGeneratorTier3 : BaseTeamUpgrade
	{
		public override string Name => "Crystal Generator";
		public override Color Color => UI.ColorPalette.Resources;
		public override string Description => "Your team's generator occasionally spawns Crystals.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 12
		};
		public override string Group => "generator";
		public override int Tier => 3;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/generator_3.png";
		}
	}
}
