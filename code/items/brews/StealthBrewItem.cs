
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class StealthBrewItem : BrewItem
	{
		public override string ConsumeEffect => "particles/gameplay/brews/stealth/stealth_brew.vpcf";
		public override string UniqueId => "item_brew_stealth";
		public override string Description => "Provides invisibility for 30 seconds when consumed. Attacking or being damaged will nullify the effect.";
		public override string Icon => "textures/items/brew_stealth.png";
		public override string Name => "Stealth Brew";

		public override void OnActivated( Player player )
		{
			player.GiveBuff( new StealthBuff() );
			base.OnActivated( player );
		}
	}
}
