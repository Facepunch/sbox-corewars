using Sandbox;

namespace Facepunch.CoreWars;

public partial class BackpackContainer : InventoryContainer
{
	public BackpackContainer() : base()
	{
		SetSlotLimit( 24 );
	}

	public override InventoryContainer GetTransferTarget( InventoryItem item )
	{
		var storage = UI.Storage.Current;

		if ( storage.IsOpen )
		{
			return storage.Container;
		}

		var equipment = CoreWarsPlayer.Me.Equipment;

		if ( item is ArmorItem && equipment.CouldTakeAny( item ) )
		{
			return equipment;
		}

		return CoreWarsPlayer.Me.Hotbar;
	}
}
