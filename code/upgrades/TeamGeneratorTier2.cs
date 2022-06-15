using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class TeamGeneratorTier2 : BaseTeamUpgrade
	{
		public override string Name => "+100% Generator Speed";
		public override Color Color => ColorPalette.Resources;
		public override string Description => "Increase speed of your team's generator by 100%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 8
		};
		public override string Group => "generator";
		public override int Tier => 2;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/generator_2.png";
		}
	}
}
