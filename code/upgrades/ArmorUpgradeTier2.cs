using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorUpgradeTier2 : BaseTeamUpgrade
	{
		public override string Name => "+40% Armor";
		public override Color Color => UI.ColorPalette.Armor;
		public override string Description => "Increase armor protection for your team by 40%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 8
		};
		public override string Group => "armor";
		public override int Tier => 2;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/armor_2.png";
		}
	}
}
