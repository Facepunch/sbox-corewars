using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class DamageUpgradeTier1 : BaseTeamUpgrade
	{
		public override string Name => "+20% Damage";
		public override string Description => "Increase damage dealt by your team by 20%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 6
		};
		public override string Group => "damage";
		public override int Tier => 1;

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/damage_1.png";
		}
	}
}
