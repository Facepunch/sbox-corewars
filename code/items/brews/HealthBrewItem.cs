
using Sandbox;
using System.Collections.Generic;
using System.IO;
using System;

namespace Facepunch.CoreWars
{
	public class HealthBrewItem : BrewItem
	{
		public override string ConsumeEffect => "particles/gameplay/brews/health/health_brew.vpcf";
		public override string UniqueId => "item_brew_health";
		public override string Description => "Restore 30% of maximum health when consumed.";
		public override string Icon => "textures/items/brew_health.png";
		public override string Name => "Health Brew";

		public override void OnActivated( Player player )
		{
			player.Health += 30f;
			player.Health = Math.Min( player.Health, 100f );
			player.ReduceStamina( 50f );

			base.OnActivated( player );
		}

		protected override void BuildTags( HashSet<string> tags )
		{
			tags.Add( "uses_stamina" );
			base.BuildTags( tags );
		}
	}
}
