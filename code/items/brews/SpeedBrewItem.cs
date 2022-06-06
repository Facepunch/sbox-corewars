using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public abstract class SpeedBrewItem : BrewItem
	{
		public override string Icon => "textures/items/brew_speed.png";
		public override string Name => "Speed Brew";

		public override void OnConsumed( Player player )
		{
			player.GiveBuff( new SpeedBuff() );
			base.OnConsumed( player );
		}
	}
}
