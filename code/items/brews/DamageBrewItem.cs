using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_brew_damage" )]
	public class DamageBrewItem : BrewItem
	{
		public override string ConsumeEffect => "particles/gameplay/brews/damage/damage_brew.vpcf";
		public override string Icon => "textures/items/brew_damage.png";
		public override string Name => "Damage Brew";

		public override void OnConsumed( Player player )
		{
			player.GiveBuff( new DamageBuff() );
			base.OnConsumed( player );
		}
	}
}
