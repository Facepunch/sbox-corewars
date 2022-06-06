﻿using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_brew_stealth" )]
	public class StealthBrewItem : BrewItem
	{
		public override string Icon => "textures/items/brew_stealth.png";
		public override string Name => "Stealth Brew";

		public override void OnConsumed( Player player )
		{
			player.GiveBuff( new StealthBuff() );
			base.OnConsumed( player );
		}
	}
}
