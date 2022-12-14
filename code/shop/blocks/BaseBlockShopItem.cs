using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Facepunch.Voxels;


namespace Facepunch.CoreWars
{
	public abstract class BaseBlockShopItem : BaseShopItem
	{
		public override string Name => GetBlockType( Game.LocalPawn as CoreWarsPlayer ).FriendlyName;
		public override string Description => GetBlockType( Game.LocalPawn as CoreWarsPlayer ).Description;
		public override Color Color => UI.ColorPalette.Blocks;
		public override int SortOrder => 2;

		public virtual BlockType GetBlockType( CoreWarsPlayer player )
		{
			return null;
		}

		public override string GetIcon( CoreWarsPlayer player )
		{
			var block = GetBlockType( player );
			if ( !string.IsNullOrEmpty( block.Icon ) ) return block.Icon;
			return $"textures/blocks/corewars/color/{block.DefaultTexture}.png";
		}

		public override Color GetIconTintColor( CoreWarsPlayer player )
		{
			var block = GetBlockType( player );
			return block.TintColor;
		}

		public override void OnPurchased( CoreWarsPlayer player )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			var block = GetBlockType( player );

			item.StackSize = (ushort)Quantity;
			item.BlockId = block.BlockId;

			player.TryGiveItem( item );
		}
	}
}
