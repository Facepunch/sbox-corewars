using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BaseShopItem : IPurchasableItem
	{
		public virtual Type RequiredUpgradeType => null;
		public virtual ItemTag[] Tags => new ItemTag[0];
		public virtual string Description => string.Empty;
		public virtual string Name => string.Empty;
		public virtual Dictionary<Type, int> Costs => new();
		public virtual Color Color => Color.White;
		public virtual int SortOrder => 1;
		public virtual int Quantity => 1;

		public virtual bool CanAfford( Player player )
		{
			foreach ( var kv in Costs )
			{
				var count = player.GetResourceCount( kv.Key );

				if ( count < kv.Value )
					return false;
			}

			return true;
		}

		public virtual bool IsLocked( Player player )
		{
			var core = player.Team.GetCore();
			if ( !core.IsValid() ) return false;

			if ( RequiredUpgradeType != null )
			{
				if ( !core.HasUpgrade( RequiredUpgradeType ) )
					return true;
			}

			return false;
		}

		public virtual string GetIcon( Player player )
		{
			return string.Empty;
		}

		public virtual bool CanPurchase( Player player )
		{
			var core = player.Team.GetCore();
			if ( !core.IsValid() ) return false;

			return true;
		}

		public virtual void OnPurchased( Player player )
		{

		}
	}
}
