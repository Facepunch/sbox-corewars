using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorUpgradeTier1 : BaseTeamUpgrade
	{
		public override string Name => "+20% Armor";
		public override Color Color => ColorPalette.Armor;
		public override string Description => "Increase armor protection for your team by 20%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 4
		};
		public override string Group => "armor";
		public override int Tier => 1;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/armor_1.png";
		}
	}
}
