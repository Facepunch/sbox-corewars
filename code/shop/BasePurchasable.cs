using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BasePurchasable : BaseNetworkable
	{
		public virtual string Description => string.Empty;
		public virtual string Name => string.Empty;
		public virtual Dictionary<Type, int> Costs => new();
		public virtual int Quantity => 1;

		public virtual bool CanAfford( Player player )
		{
			foreach ( var kv in Costs )
			{
				var sum = player.FindItems( kv.Key ).Sum( i => i.StackSize );

				if ( sum < kv.Value )
					return false;
			}

			return true;
		}

		public virtual string GetIcon( Player player )
		{
			return string.Empty;
		}

		public virtual bool CanPurchase( Player player )
		{
			return true;
		}

		public virtual void OnPurchased( Player player )
		{

		}
	}
}
