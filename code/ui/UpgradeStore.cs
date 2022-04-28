﻿using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class UpgradeStore : Panel, IDialog
	{
		public static UpgradeStore Current { get; private set; }

		public Panel ItemContainer { get; set; }
		public TeamUpgradesNPC NPC { get; set; }
		public bool IsOpen { get; set; }

		public UpgradeStore()
		{
			Current = this;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			IDialog.Activate( this );
			IsOpen = true;
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			IDialog.Deactivate( this );
			IsOpen = false;
		}

		public void SetNPC( TeamUpgradesNPC npc )
		{
			NPC = npc;

			ItemContainer.DeleteChildren( true );

			foreach ( var item in npc.Upgrades )
			{
				var panel = ItemContainer.AddChild<PurchasableItem>( "item" );
				panel.OnPurchaseClicked += OnItemPurchased;
				panel.SetItem( item );
			}
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player ) return;

			if ( NPC.IsValid() )
			{
				var distance = NPC.Position.Distance( player.Position );

				if ( distance >= 300f )
				{
					Close();
					return;
				}
			}

			base.Tick();
		}

		protected virtual void OnItemPurchased( IPurchasableItem item )
		{
			Player.BuyUpgradeCmd( NPC.NetworkIdent, item.GetType().Name );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			BindClass( "hidden", () => !IsOpen );
		}
	}
}
