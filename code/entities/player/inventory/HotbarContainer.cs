using Sandbox;

namespace Facepunch.CoreWars;

public partial class HotbarContainer : InventoryContainer
{
	public HotbarContainer() : base()
	{
		SetSlotLimit( 8 );
	}

	public override InventoryContainer GetTransferTarget( InventoryItem item )
	{
		if ( Entity is CoreWarsPlayer player )
		{
			return UI.Storage.Current.IsOpen ? UI.Storage.Current.Container : CoreWarsPlayer.Me.Backpack;
		}

		return base.GetTransferTarget( item );
	}
}
