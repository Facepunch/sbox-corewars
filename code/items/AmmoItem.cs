using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_ammo" )]
	public class AmmoItem : InventoryItem
	{
		public AmmoType AmmoType { get; set; }

		public override string Description => AmmoType.GetDescription();
		public override bool RemoveOnDeath => true;
		public override Color Color => ColorPalette.Ammo;
		public override ushort MaxStackSize => 60;
		public override string Name => AmmoType.ToString();
		public override string Icon => $"textures/items/ammo_{AmmoType.ToString().ToLower()}.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return (other is AmmoItem item && item.AmmoType == AmmoType);
		}

		public override void Write( BinaryWriter writer )
		{
			writer.Write( (int)AmmoType );
			base.Write( writer );
		}

		public override void Read( BinaryReader reader )
		{
			AmmoType = (AmmoType)reader.ReadInt32();
			base.Read( reader );
		}
	}
}
