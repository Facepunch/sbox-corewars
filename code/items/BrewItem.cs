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
		public virtual string ConsumeEffect => null;

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public virtual void OnConsumed( Player player )
		{
			using ( Prediction.Off() )
			{
				if ( !string.IsNullOrEmpty( ConsumeSound ) )
				{
					player.PlaySound( ConsumeSound );
				}

				if ( !string.IsNullOrEmpty( ConsumeEffect ) )
				{
					var effect = Particles.Create( ConsumeEffect, player );
					effect.SetEntity( 0, player );
				}
			}

			StackSize--;

			if ( StackSize <= 0 )
				Remove();
		}
	}
}
