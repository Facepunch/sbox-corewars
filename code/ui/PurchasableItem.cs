using Sandbox;
using Sandbox.UI;
using System;
using Sandbox.UI.Construct;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class PurchasableItem : Panel
	{
		public Panel Icon { get; set; }
		public Panel CostContainer { get; set; }
		public IPurchasableItem Item { get; private set; }
		public Action<IPurchasableItem> OnPurchaseClicked { get; set; }
		public Button PurchaseButton { get; set; }
		public string QuantityText => GetQuantityText();

		private TimeUntil NextCheckState { get; set; }

		public void SetItem( IPurchasableItem item )
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
					var itemType = TypeLibrary.Create<ResourceItem>( kv.Key );
					if ( itemType == null ) continue;

					var itemCost = kv.Value;
					var panel = CostContainer.Add.Panel( "cost" );

					panel.Add.Image( itemType.Icon, "icon" );
					panel.Add.Label( $"x{itemCost}", "value" );
				}
			}

			UpdateState();
		}

		public void DoPurchase()
		{
			if ( IsPurchaseDisabled() ) return;
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

		public override void Tick()
		{
			UpdateState();
			base.Tick();
		}

		private string GetQuantityText()
		{
			if ( Item != null && Item.Quantity > 0 )
			{
				return $"x{Item.Quantity}";
			}

			return string.Empty;
		}

		private void UpdateState()
		{
			PurchaseButton.SetClass( "disabled", IsPurchaseDisabled() );
			SetClass( "hidden", IsHidden() );
		}
	}
}
