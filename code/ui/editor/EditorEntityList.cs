using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorEntityList : Panel
	{
		public static EditorEntityList Current { get; private set; }

		public Panel Items { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorEntityList();
			Current.PopulateItems();

			Game.Hud.AddChild( Current );
		}

		public EditorEntityList()
		{
			AcceptsFocus = true;
			Focus();
		}

		public void PopulateItems()
		{
			Items.DeleteChildren();

			var attributes = Library.GetAttributes<EditorEntityLibraryAttribute>();

			foreach ( var attribute in attributes )
			{
				var item = Items.AddChild<EditorEntityItem>();
				item.SetAttribute( attribute );
				item.OnSelected = () => OnItemSelected( item );
			}
		}

		public override void OnButtonTyped( string button, KeyModifiers km )
		{
			if ( button == "escape" )
			{
				Blur();
			}

			base.OnButtonTyped( button, km );
		}

		protected virtual void OnItemSelected( EditorEntityItem item )
		{
			EntitiesTool.ChangeLibraryAttributeCmd( item.Attribute.Name );
			Delete();
		}

		protected override void OnBlur( PanelEvent e )
		{
			Delete();

			base.OnBlur( e );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
