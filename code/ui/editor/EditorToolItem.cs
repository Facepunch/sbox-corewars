using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorToolItem : Panel
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public ushort Slot { get; set; }
		public bool IsSelected { get; set; }
		public int LibraryId { get; set; }

		public EditorToolItem() { }

		public void SetLibraryItem( LibraryAttribute item )
		{
			LibraryId = item.Identifier;
			Description = item.Description;
			Name = item.Name;
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "selected", () => IsSelected );
			base.PostTemplateApplied();
		}
	}
}
