using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_block" )]
	public class BlockItem : InventoryItem
	{
		public byte BlockId { get; set; }

		public override string GetName()
		{
			if ( VoxelWorld.Current.IsValid() )
				return VoxelWorld.Current.GetBlockType( BlockId ).FriendlyName;
			else
				return "INVALID_BLOCK";
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return (other is BlockItem item && item.BlockId == BlockId);
		}

		public override string GetIcon()
		{
			if ( VoxelWorld.Current == null ) return string.Empty;

			var block = VoxelWorld.Current.GetBlockType( BlockId );
			if ( string.IsNullOrEmpty( block.DefaultTexture ) ) return string.Empty;

			return $"textures/blocks/color/{ block.DefaultTexture }.png";
		}

		public override void Write( BinaryWriter writer )
		{
			writer.Write( BlockId );
			base.Write( writer );
		}

		public override void Read( BinaryReader reader )
		{
			BlockId = reader.ReadByte();
			base.Read( reader );
		}
	}
}
