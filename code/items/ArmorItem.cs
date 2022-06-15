using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorItem : InventoryItem
	{
		public override bool CanBeDropped => false;

		public virtual float DamageMultiplier => 1f;
		public virtual ArmorSlot ArmorSlot => ArmorSlot.None;
		public override Color Color => ColorPalette.Armor;
		public virtual string ModelName => string.Empty;
		public virtual int Tier => 0;

		public override bool CanStackWith( InventoryItem other )
		{
			return false;
		}
	}
}
