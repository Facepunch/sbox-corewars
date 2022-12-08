﻿
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class DamageBrewItem : BrewItem
	{
		public override string ConsumeEffect => "particles/gameplay/brews/damage/damage_brew.vpcf";
		public override string UniqueId => "item_brew_damage";
		public override string Description => "Gives a boost to damage output for 30 seconds when consumed.";
		public override string Icon => "textures/items/brew_damage.png";
		public override string Name => "Damage Brew";

		public override void OnActivated( Player player )
		{
			player.GiveBuff( new DamageBuff() );
			base.OnActivated( player );
		}
	}
}
