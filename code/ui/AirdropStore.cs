using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class AirdropStore : Panel, IDialog
	{
		public static AirdropStore Current { get; private set; }

		public string IronAmount => GetResourceCount<IronItem>().ToString();
		public string GoldAmount => GetResourceCount<GoldItem>().ToString();
		public string CrystalAmount => GetResourceCount<CrystalItem>().ToString();
		public Panel ItemContainer { get; set; }
		public Airdrop Airdrop { get; set; }
		public bool IsOpen { get; set; }

		public AirdropStore()
		{
			Current = this;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			PlaySound( "itemstore.open" );
			IDialog.Activate( this );
			IsOpen = true;
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			IDialog.Deactivate( this );
			IsOpen = false;
		}

		public void SetAirdrop( Airdrop airdrop )
		{
			Airdrop = airdrop;

			ItemContainer.DeleteChildren( true );

			foreach ( var item in airdrop.Items )
			{
				var panel = ItemContainer.AddChild<PurchasableItem>( "item" );
				panel.OnPurchaseClicked += OnItemPurchased;
				panel.SetItem( item );
			}
		}

		public void SetItems( List<BaseShopItem> items )
		{

		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player ) return;

			if ( Airdrop.IsValid() )
			{
				var distance = Airdrop.Position.Distance( player.Position );

				if ( distance >= 300f )
				{
					Close();
					return;
				}
			}

			base.Tick();
		}

		protected int GetResourceCount<T>() where T : ResourceItem
		{
			if ( Local.Pawn is Player player )
			{
				return player.GetResourceCount<T>();
			}

			return 0;
		}

		protected virtual void OnItemPurchased( IPurchasableItem item )
		{
			Player.BuyItemCmd( Airdrop.NetworkIdent, item.GetType().Name );
			Audio.Play( "item.purchase" );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			if ( Airdrop.IsValid() )
			{
				SetAirdrop( Airdrop );
			}

			BindClass( "hidden", () => !IsOpen );
		}
	}
}
