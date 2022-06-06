using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class BrewItem : InventoryItem
	{
		public override bool CanBeDropped => false;
		public override ushort MaxStackSize => 4;
		public virtual string ConsumeSound => "brew.consume";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public virtual void OnConsumed( Player player )
		{
			if ( !string.IsNullOrEmpty( ConsumeSound ) )
			{
				using ( Prediction.Off() )
				{
					player.PlaySound( ConsumeSound );
				}
			}

			StackSize--;

			if ( StackSize <= 0 )
				Remove();
		}
	}
}
