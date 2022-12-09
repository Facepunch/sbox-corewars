using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class DamageUpgradeTier2 : BaseTeamUpgrade
	{
		public override string Name => "+35% Damage";
		public override Color Color => UI.ColorPalette.Weapons;
		public override string Description => "Increase damage dealt by your team by 35%.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 12
		};
		public override string Group => "damage";
		public override int Tier => 2;

		public override string GetIcon( CoreWarsPlayer player )
		{
			return "textures/upgrades/damage_2.png";
		}
	}
}
