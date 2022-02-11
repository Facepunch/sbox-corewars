using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Voxel;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class InventorySlot : Panel
	{
		public InventoryContainer Container { get; set; }
		public ushort Slot { get; set; }
		public InventoryItem Item { get; set; }
		public bool IsSelected { get; set; }
		public string StackSize => (Item.IsValid() && Item.StackSize > 1) ? Item.StackSize.ToString() : string.Empty;

		public InventorySlot() { }

		public void SetItem( InventoryItem item )
		{
			Item = item;

			if ( !item.IsValid() )
			{
				Style.BackgroundImage = null;
				return;
			}

			var icon = item.GetIcon();

			if ( !string.IsNullOrEmpty( icon ) )
			{
				var texture = Texture.Load( FileSystem.Mounted, icon );
				Style.SetBackgroundImage( texture );
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
