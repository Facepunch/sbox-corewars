using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmoryUpgrade : BaseTeamUpgrade
	{
		public override string Name => "Armory";
		public override Color Color => ColorPalette.Weapons;
		public override string Description => "Unlock new weapons and items to purchase.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( GoldItem )] = 4
		};

		public override string GetIcon( Player player )
		{
			return "textures/upgrades/armory.png";
		}
	}
}
