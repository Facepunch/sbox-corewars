using Facepunch.CoreWars.Utility;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

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
		public bool WasDisabled { get; private set; }
		public ItemTag[] Tags { get; private set; }
		public Color Color => Item.Color;

		public void SetItem( IPurchasableItem item )
		{
			Item = item;

			if ( Local.Pawn is Player player )
			{
				if ( item.IsLocked( player ) )
				{
					Rand.SetSeed( item.GetHashCode() );

					var symbolCharacters = new string[] { "𐆓", "𐆕", "𐆖", "𐆗", "𐆙", "𐆚", "𐆛", "𐆜", "𐆠", "𐅍", "𐅍", "𐅡", "𐅍", "𐹳", "𐅢" };
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
					Tags = new ItemTag[0];

					Icon.Style.SetBackgroundImage( "textures/ui/unknown.png" );
				}
				else
				{
					Description = item.Description;
					Name = item.Name;
					Tags = item.Tags;

					var icon = item.GetIcon( player );

					if ( !string.IsNullOrEmpty( icon ) )
						Icon.Style.SetBackgroundImage( icon );
					else
						Icon.Style.BackgroundImage = null;
				}

				SetClass( "is-block", item is BaseBlockShopItem );
			}

			UpdateState( true );
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
					tooltip.DescriptionLabel.Style.FontFamily = "NotoSansSymbols2-Regular";
					tooltip.NameLabel.Style.FontFamily = "NotoSansSymbols2-Regular";
					AddRequirementToTooltip( tooltip );
				}
				else
				{
					AddCostsToTooltip( tooltip );
				}
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
			if ( Local.Pawn is not Player player )
				return;

			var costContainer = tooltip.Container.AddChild<Panel>( "costs" );

			foreach ( var kv in Item.Costs )
			{
				var itemType = TypeLibrary.Create<ResourceItem>( kv.Key );
				if ( itemType == null ) continue;

				var itemCost = kv.Value;
				var panel = costContainer.Add.Panel( "cost" );

				panel.Add.Image( itemType.Icon, "icon" );

				var value = panel.Add.Label( $"x{itemCost}", "value" );
				value.SetClass( "unaffordable", player.GetResourceCount( kv.Key ) < kv.Value );
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

		private void UpdateState( bool shouldForceUpdate = false )
		{
			var isDisabled = IsPurchaseDisabled();

			SetClass( "disabled", isDisabled );
			SetClass( "hidden", IsHidden() );

			// Early out to avoid processing if our state is the same.
			if ( !shouldForceUpdate && isDisabled == WasDisabled )
				return;

			if ( isDisabled )
			{
				Style.SetLinearGradientBackground( Color.Black, 0.5f, new Color( 0.2f ), 0.5f );
				Style.BorderColor = Color.Black.WithAlpha( 0.6f );
			}
			else
			{
				if ( Item.Color == Color.White )
					Style.SetLinearGradientBackground( Color.Black, 0.5f, new Color( 0.2f ), 0.5f );
				else
					Style.SetLinearGradientBackground( Item.Color, 0.5f, new Color( 0.2f ), 0.5f );

				Style.BorderColor = Item.Color.WithAlpha( 0.6f );
			}

			WasDisabled = isDisabled;
		}
	}
}
