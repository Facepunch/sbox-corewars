using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorUpgradeTier3 : BaseTeamUpgrade
	{
		public override string Name => "+60% Armor";
		public override Color Color => UI.ColorPalette.Armor;
		public override string Description => "Increase armor protection for your team by 60%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 20
		};
		public override string Group => "armor";
		public override int Tier => 3;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/armor_3.png";
		}
	}
}
