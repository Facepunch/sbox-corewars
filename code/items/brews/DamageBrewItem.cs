using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public abstract class DamageBrewItem : BrewItem
	{
		public override string Icon => "textures/items/brew_damage.png";
		public override string Name => "Damage Brew";

		public override void OnConsumed( Player player )
		{
			player.GiveBuff( new DamageBuff() );
			base.OnConsumed( player );
		}
	}
}
