using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_watergun" )]
	public class WatergunItem : WeaponItem
	{
		public override string WeaponName => "weapon_watergun";
		public override string Description => "A simple tool for neutralizing Vortex Bombs.";
		public override string Group => "watergun";
		public override int Tier => 1;
		public override string Icon => "textures/items/weapon_watergun.png";
		public override string Name => "Watergun";
		public override bool RemoveOnDeath => true;
	}
}
