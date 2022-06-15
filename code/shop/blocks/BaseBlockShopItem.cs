using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public abstract class BaseBlockShopItem : BaseShopItem
	{
		public override string Name => GetBlockType( Local.Pawn as Player ).FriendlyName;
		public override string Description => GetBlockType( Local.Pawn as Player ).Description;
		public override Color Color => ColorPalette.Blocks;
		public override int SortOrder => 2;

		public virtual BlockType GetBlockType( Player player )
		{
			return null;
		}

		public override string GetIcon( Player player )
		{
			var block = GetBlockType( player );
			if ( !string.IsNullOrEmpty( block.Icon ) ) return block.Icon;
			return $"textures/blocks/corewars/color/{block.DefaultTexture}.png";
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			var block = GetBlockType( player );

			item.StackSize = (ushort)Quantity;
			item.BlockId = block.BlockId;

			player.TryGiveItem( item );
		}
	}
}
