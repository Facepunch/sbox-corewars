using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class BreweryUpgrade : BaseTeamUpgrade
	{
		public override string Name => "Brewery";
		public override Color Color => ColorPalette.Brews;
		public override string Description => "Unlock special consumable brews.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 4
		};

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/brewery.png";
		}
	}
}
