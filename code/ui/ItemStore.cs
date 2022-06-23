using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class ItemStore : Panel, IDialog
	{
		public static ItemStore Current { get; private set; }

		public string IronAmount => GetResourceCount<IronItem>().ToString();
		public string GoldAmount => GetResourceCount<GoldItem>().ToString();
		public string CrystalAmount => GetResourceCount<CrystalItem>().ToString();
		public Panel ItemContainer { get; set; }
		public ItemStoreEntity Entity { get; set; }
		public bool IsOpen { get; set; }

		public ItemStore()
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

		public void SetEntity( ItemStoreEntity entity )
		{
			Entity = entity;

			ItemContainer.DeleteChildren( true );

			entity.Items.Sort( ( a, b ) => a.SortOrder.CompareTo( b.SortOrder ) );

			foreach ( var item in entity.Items )
			{
				var panel = ItemContainer.AddChild<PurchasableItem>( "item" );
				panel.OnPurchaseClicked += OnItemPurchased;
				panel.SetItem( item );
			}
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player ) return;

			if ( Entity.IsValid() )
			{
				var distance = Entity.Position.Distance( player.Position );

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
			Player.BuyItemCmd( Entity.NetworkIdent, item.GetType().Name );
			Util.Play( "item.purchase" );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			if ( Entity.IsValid() )
			{
				SetEntity( Entity );
			}

			BindClass( "hidden", () => !IsOpen );
		}
	}
}
