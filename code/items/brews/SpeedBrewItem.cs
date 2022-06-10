using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_brew_speed" )]
	public class SpeedBrewItem : BrewItem
	{
		public override string ConsumeEffect => "particles/gameplay/brews/speed/speed_brew.vpcf";
		public override string Icon => "textures/items/brew_speed.png";
		public override string Name => "Speed Brew";

		public override void OnConsumed( Player player )
		{
			player.GiveBuff( new SpeedBuff() );
			base.OnConsumed( player );
		}
	}
}
