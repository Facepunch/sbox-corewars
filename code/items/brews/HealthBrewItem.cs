﻿using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;
using System;

namespace Facepunch.CoreWars
{
	[Library( "item_brew_health" )]
	public class HealthBrewItem : BrewItem
	{
		public override string Icon => "textures/items/brew_health.png";
		public override string Name => "Health Brew";

		public override void OnConsumed( Player player )
		{
			player.Health += 30f;
			player.Health = Math.Min( player.Health, 100f );

			base.OnConsumed( player );
		}
	}
}