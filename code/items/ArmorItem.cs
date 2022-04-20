using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorItem : InventoryItem
	{
		public virtual float DamageMultiplier => 1f;
		public virtual ArmorSlot ArmorSlot => ArmorSlot.None;

		public override bool CanStackWith( InventoryItem other )
		{
			return false;
		}
	}
}
