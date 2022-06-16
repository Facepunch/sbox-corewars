using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_neutralizer" )]
	public class NeutralizerItem : WeaponItem
	{
		public override string WeaponName => "weapon_neutralizer";
		public override string Description => "A simple tool for neutralizing Vortex Bombs.";
		public override Color Color => ColorPalette.Tools;
		public override string Group => "neutralizer";
		public override int Tier => 1;
		public override string Icon => "textures/items/weapon_neutralizer.png";
		public override string Name => "Neutralizer";
		public override bool RemoveOnDeath => true;
		public override ItemTag[] Tags => new ItemTag[0];
	}
}
