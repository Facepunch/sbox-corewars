using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_ammo" )]
	public class AmmoItem : InventoryItem
	{
		public AmmoType AmmoType { get; set; }

		public override string GetName()
		{
			return AmmoType.ToString();
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return (other is AmmoItem item && item.AmmoType == AmmoType);
		}

		public override string GetIcon()
		{
			return string.Empty;
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
