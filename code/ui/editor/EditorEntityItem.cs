using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;
using System;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorEntityItem : Panel
	{
		public EditorEntityLibraryAttribute Attribute { get; private set; }

		public Action OnSelected { get; set; }
		public string Name => Attribute.Name;

		public EditorEntityItem() { }

		public void SetAttribute( EditorEntityLibraryAttribute attribute )
		{
			Attribute = attribute;
		}

		protected override void OnClick( MousePanelEvent e )
		{
			OnSelected?.Invoke();
			base.OnClick( e );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
