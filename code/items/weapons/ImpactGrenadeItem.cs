using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_impact_grenade" )]
	public class ImpactGrenadeItem : WeaponItem
	{
		public override bool RemoveOnDeath => true;
		public override string WeaponName => "weapon_impact_grenade";
		public override string Icon => "textures/items/weapon_impact_grenade.png";
		public override string Name => "Impact Grenade";
	}
}
