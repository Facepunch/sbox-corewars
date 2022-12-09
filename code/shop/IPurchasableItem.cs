using System;
using System.Collections.Generic;
using Sandbox;

namespace Facepunch.CoreWars
{
	public interface IPurchasableItem
	{
		public string Name { get; }
		public IReadOnlySet<string> Tags { get; }
		public string Description { get; }
		public Color Color { get; }
		public Dictionary<Type, int> Costs { get; }
		public int Quantity { get; }

		public bool CanAfford( CoreWarsPlayer player );
		public string GetIcon( CoreWarsPlayer player );
		public Color GetIconTintColor( CoreWarsPlayer player );
		public bool IsLocked( CoreWarsPlayer player );
		public bool CanPurchase( CoreWarsPlayer player );
		public void OnPurchased( CoreWarsPlayer player );
	}
}
