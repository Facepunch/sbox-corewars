using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.CoreWars
{
	public abstract class BaseTeamUpgrade : IPurchasableItem, IValid
	{
		public virtual string Description => string.Empty;
		public virtual ItemTag[] Tags => new ItemTag[0];
		public virtual string Name => string.Empty;
		public virtual string Group => string.Empty;
		public virtual Dictionary<Type, int> Costs => new();
		public virtual Color Color => Color.White;
		public virtual int Quantity => 0;
		public virtual int Tier => 1;

		public bool IsValid => true;

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

		public virtual string GetIcon( Player player )
		{
			return string.Empty;
		}

		public virtual Color GetIconTintColor( Player player )
		{
			return Color.White;
		}

		public virtual bool IsLocked( Player player )
		{
			var core = player.Team.GetCore();
			if ( !core.IsValid() ) return false;
			return false;
		}

		public virtual bool CanPurchase( Player player )
		{
			var core = player.Team.GetCore();
			if ( !core.IsValid() ) return false;

			if ( core.HasUpgrade( GetType() ) )
				return false;

			if ( !string.IsNullOrEmpty( Group ) )
			{
				if ( Tier > 1 )
					return core.HasPreviousUpgrade( Group, Tier );
				else
					return !core.HasNewerUpgrade( Group, Tier );
			}

			return true;
		}

		public virtual void OnPurchased( Player player )
		{
			var core = player.Team.GetCore();

			if ( core.IsValid() )
			{
				core.AddUpgrade( this );
			}
		}
	}
}
