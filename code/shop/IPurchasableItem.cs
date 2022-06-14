using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	public interface IPurchasableItem
	{
		public string Name { get; }
		public string Description { get; }
		public Color Color { get; }
		public Dictionary<Type, int> Costs { get; }
		public int Quantity { get; }

		public bool CanAfford( Player player );
		public string GetIcon( Player player );
		public bool IsLocked( Player player );
		public bool CanPurchase( Player player );
		public void OnPurchased( Player player );
	}
}
