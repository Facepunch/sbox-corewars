using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BaseTeamUpgrade : BaseNetworkable
	{
		public virtual Type PreviousUpgradeType => null;
		public virtual string Description => string.Empty;
		public virtual string Name => string.Empty;
		public virtual Dictionary<Type, int> Costs => new();

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
			var core = player.Team.GetCore();
			if ( !core.IsValid() ) return false;

			if ( core.Upgrades.Any( u => u.GetType() == GetType() ) )
				return false;

			if ( PreviousUpgradeType != null )
				return core.Upgrades.Any( u => u.GetType() == PreviousUpgradeType );

			return true;
		}

		public virtual void OnPurchased( Player player )
		{
			var core = player.Team.GetCore();

			if ( core.IsValid() )
			{
				core.Upgrades.Add( this );
			}
		}
	}
}
