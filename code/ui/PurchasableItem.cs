using Sandbox;
using Sandbox.UI;
using System;
using Sandbox.UI.Construct;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class PurchasableItem : Panel, ITooltipProvider
	{
		public Panel Icon { get; set; }
		public IPurchasableItem Item { get; private set; }
		public Action<IPurchasableItem> OnPurchaseClicked { get; set; }
		public Button PurchaseButton { get; set; }
		public string QuantityText => GetQuantityText();
		public string Name { get; private set; }
		public string Description { get; private set; }
		public Color Color => Item.Color;

		public void SetItem( IPurchasableItem item )
		{
			Item = item;

			if ( Local.Pawn is Player player )
			{
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

					Description = randomDescription;
					Name = randomName;

					Icon.Style.SetBackgroundImage( "textures/ui/unknown.png" );
				}
				else
				{
					Description = item.Description;
					Name = item.Name;

					var icon = item.GetIcon( player );

					if ( !string.IsNullOrEmpty( icon ) )
						Icon.Style.SetBackgroundImage( icon );
					else
						Icon.Style.BackgroundImage = null;
				}

				Style.BorderColor = Item.Color.WithAlpha( 0.6f );
			}

			UpdateState();
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

		protected override void OnClick( MousePanelEvent e )
		{
			if ( IsPurchaseDisabled() ) return;
			OnPurchaseClicked?.Invoke( Item );
			base.OnClick( e );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			if ( Local.Pawn is not Player player )
				return;

			if ( Item != null )
			{
				var tooltip = Tooltip.Show( this );
				tooltip.Style.FontColor = Item.Color;

				if ( Item.IsLocked( player ) )
				{
					tooltip.DescriptionLabel.Style.FontFamily = "Wingdings";
					tooltip.NameLabel.Style.FontFamily = "Wingdings";
					AddRequirementToTooltip( tooltip );
				}

				AddCostsToTooltip( tooltip );
			}

			base.OnMouseOver( e );
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			Tooltip.Hide( this );
			base.OnMouseOut( e );
		}

		private void AddRequirementToTooltip( Tooltip tooltip )
		{
			tooltip.Container.Add.Label( $"Team Upgrade Required", "requirement" );
		}

		private void AddCostsToTooltip( Tooltip tooltip )
		{
			var costContainer = tooltip.Container.AddChild<Panel>( "costs" );

			foreach ( var kv in Item.Costs )
			{
				var itemType = TypeLibrary.Create<ResourceItem>( kv.Key );
				if ( itemType == null ) continue;

				var itemCost = kv.Value;
				var panel = costContainer.Add.Panel( "cost" );

				panel.Add.Image( itemType.Icon, "icon" );
				panel.Add.Label( $"x{itemCost}", "value" );
			}
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
			SetClass( "disabled", IsPurchaseDisabled() );
			SetClass( "hidden", IsHidden() );
		}
	}
}
