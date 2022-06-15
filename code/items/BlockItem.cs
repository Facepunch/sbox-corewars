﻿using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_block" )]
	public class BlockItem : InventoryItem
	{
		public byte BlockId { get; set; }

		public override ushort MaxStackSize => 64;
		public override bool RemoveOnDeath => true;
		public override Color Color => ColorPalette.Blocks;
		public override string Description
		{
			get
			{
				var world = VoxelWorld.Current;
				if ( !world.IsValid() ) return "Invalid";
				return world.GetBlockType( BlockId ).Description;
			}
		}

		public override string Name
		{
			get
			{
				var world = VoxelWorld.Current;
				if ( !world.IsValid() ) return "Invalid";
				return world.GetBlockType( BlockId ).FriendlyName;
			}
		}

		public override string Icon
		{
			get
			{
				var world = VoxelWorld.Current;
				if ( !world.IsValid() ) return string.Empty;

				var block = world.GetBlockType( BlockId );

				if ( !string.IsNullOrEmpty( block.Icon ) ) return block.Icon;
				if ( string.IsNullOrEmpty( block.DefaultTexture ) ) return string.Empty;

				return $"textures/blocks/corewars/color/{ block.DefaultTexture }.png";
			}
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return (other is BlockItem item && item.BlockId == BlockId);
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
