using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class TeamGeneratorTier1 : BaseTeamUpgrade
	{
		public override string Name => "+50% Generator Speed";
		public override Color Color => ColorPalette.Resources;
		public override string Description => "Increase speed of your team's generator by 50%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 4
		};
		public override string Group => "generator";
		public override int Tier => 1;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/generator_1.png";
		}
	}
}
