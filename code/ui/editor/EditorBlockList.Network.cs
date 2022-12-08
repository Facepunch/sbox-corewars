using Sandbox;

namespace Facepunch.CoreWars.Editor;

public partial class EditorBlockList
{
	[ConVar.Client( "cw_show_all_blocks" ), Change( nameof( OnShowAllBlocksChanged ) )]
	public static bool ShowAllBlocks { get; set; }

	private static void OnShowAllBlocksChanged( bool oldValue, bool newValue )
	{
		Current?.PopulateItems();
	}
}
