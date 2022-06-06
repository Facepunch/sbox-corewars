using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public abstract class HealthBrewItem : BrewItem
	{
		public override string Icon => "textures/items/brew_health.png";
		public override string Name => "Health Brew";

		public override void OnConsumed( Player player )
		{
			base.OnConsumed( player );
		}
	}
}
