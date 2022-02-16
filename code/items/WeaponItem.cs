using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_weapon" )]
	public class WeaponItem : InventoryItem
	{
		public string WeaponName { get; set; }
		public Weapon Weapon { get; set; }

		public override string GetName()
		{
			return Weapon.ToString();
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return false;
		}

		public override string GetIcon()
		{
			return string.Empty;
		}

		public override void Write( BinaryWriter writer )
		{
			if ( Weapon.IsValid() )
				writer.Write( Weapon.NetworkIdent );
			else
				writer.Write( 0 );

			base.Write( writer );
		}

		public override void Read( BinaryReader reader )
		{
			Weapon = (Entity.FindByIndex( reader.ReadInt32() ) as Weapon);
			base.Read( reader );
		}

		public override void OnRemoved()
		{
			if ( IsServer && Weapon.IsValid() )
			{
				Weapon.Delete();
			}

			base.OnRemoved();
		}
	}
}
