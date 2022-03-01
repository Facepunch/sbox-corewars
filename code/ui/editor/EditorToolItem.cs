using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorToolItem : Panel
	{
		public ushort Slot { get; set; }
		public bool IsSelected { get; set; }
		public EditorToolLibraryAttribute Attribute { get; private set; }
		public Image Icon { get; set; }

		public EditorToolItem() { }

		public void SetLibraryItem( EditorToolLibraryAttribute item )
		{
			Attribute = item;
			Icon?.SetTexture( Attribute.Icon );
		}

		public override void OnLayout( ref Rect layoutRect )
		{
			base.OnLayout( ref layoutRect );

			var halfWidth = layoutRect.width / 2f;
			var halfHeight = layoutRect.height / 2f;

			layoutRect.left -= halfWidth;
			layoutRect.top -= halfHeight;
			layoutRect.right -= halfWidth;
			layoutRect.bottom -= halfHeight;
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "selected", () => IsSelected );

			if ( Attribute != null )
			{
				SetLibraryItem( Attribute );
			}

			base.PostTemplateApplied();
		}
	}
}
