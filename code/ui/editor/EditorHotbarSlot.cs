using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorHotbarSlot : Panel
	{
		public ushort Slot { get; set; }
		public byte BlockId { get; set; }
		public bool IsSelected { get; set; }

		public EditorHotbarSlot() { }

		public void SetBlockId( byte blockId )
		{
			BlockId = blockId;

			var block = VoxelWorld.Current.GetBlockType( blockId );
			var icon = block.DefaultTexture;

			if ( !string.IsNullOrEmpty( icon ) )
			{
				Style.SetBackgroundImage( icon );
				Style.BackgroundSizeX = Length.Cover;
				Style.BackgroundSizeY = Length.Cover;
			}
			else
			{
				Style.BackgroundImage = null;
			}
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "selected", () => IsSelected );
			base.PostTemplateApplied();
		}
	}
}
