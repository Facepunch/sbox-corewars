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
		public Label NameLabel { get; private set; }
		public Label DescriptionLabel { get; private set; }

		public void SetItem( IPurchasableItem item )
		{
			Item = item;

			CostContainer.DeleteChildren( true );

			if ( Local.Pawn is Player player )
			{
				foreach ( var kv in item.Costs )
				{
					var itemType = TypeLibrary.Create<ResourceItem>( kv.Key );
					if ( itemType == null ) continue;

					var itemCost = kv.Value;
					var panel = CostContainer.Add.Panel( "cost" );

					panel.Add.Image( itemType.Icon, "icon" );
					panel.Add.Label( $"x{itemCost}", "value" );
				}

				if ( item.IsLocked( player ) )
				{
					var symbolCharacters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "P" };
					var randomDescription = string.Empty;
					var randomDescriptionCount = Rand.Int( 16, 24 );
					var randomName = string.Empty;
					var randomNameCount = Rand.Int( 6, 12 );

					for ( var i = 0; i < randomDescriptionCount; i++ )
					{
						randomDescription += Rand.FromArray( symbolCharacters );
					}

					for ( var i = 0; i < randomNameCount; i++ )
					{
						randomName += Rand.FromArray( symbolCharacters );
					}

					DescriptionLabel.Style.FontFamily = "Wingdings";
					DescriptionLabel.Text = randomDescription;
					NameLabel.Style.FontFamily = "Wingdings";
					NameLabel.Text = randomName;

					Icon.Style.SetBackgroundImage( "textures/ui/unknown.png" );
				}
				else
				{
					DescriptionLabel.Text = item.Description;
					NameLabel.Text = item.Name;

					var icon = item.GetIcon( player );

					if ( !string.IsNullOrEmpty( icon ) )
						Icon.Style.SetBackgroundImage( icon );
					else
						Icon.Style.BackgroundImage = null;
				}
			}

			NameLabel.Style.FontColor = item.Color;
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
				return Item.IsLocked( player ) || !Item.CanAfford( player );
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
