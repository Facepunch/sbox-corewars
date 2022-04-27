using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox.UI.Construct;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class PurchasableItem : Panel
	{
		public Panel Icon { get; set; }
		public Panel CostContainer { get; set; }
		public BaseShopItem Item { get; private set; }
		public Action<BaseShopItem> OnPurchaseClicked { get; set; }
		public Button PurchaseButton { get; set; }

		public void SetItem( BaseShopItem item )
		{
			Item = item;

			CostContainer.DeleteChildren( true );

			if ( Local.Pawn is Player player )
			{
				var icon = item.GetIcon( player );

				if ( !string.IsNullOrEmpty( icon ) )
					Icon.Style.SetBackgroundImage( icon );
				else
					Icon.Style.BackgroundImage = null;

				foreach ( var kv in item.Costs )
				{
					var itemType = Library.Create<ResourceItem>( kv.Key );
					if ( itemType == null ) continue;

					var itemCost = kv.Value;
					var panel = CostContainer.Add.Panel( "cost" );

					panel.Add.Image( itemType.Icon, "icon" );
					panel.Add.Label( $"x{itemCost}", "value" );
				}
			}
		}

		public void DoPurchase()
		{
			Log.Info( "Purchase clicked" );

			OnPurchaseClicked?.Invoke( Item );
		}

		public bool IsHidden()
		{
			if ( Item != null && Local.Pawn is Player player )
			{
				return !Item.CanPurchase( player );
			}

			return true;
		}

		public bool IsPurchaseDisabled()
		{
			if ( Item != null && Local.Pawn is Player player )
			{
				return !Item.CanAfford( player );
			}

			return true;
		}

		protected override void PostTemplateApplied()
		{
			PurchaseButton.BindClass( "disabled", IsPurchaseDisabled );
			BindClass( "hidden", IsHidden );

			base.PostTemplateApplied();
		}
	}
}
