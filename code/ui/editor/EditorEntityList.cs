using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorEntityList : Panel
	{
		public static EditorEntityList Current { get; private set; }

		public Panel Container { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorEntityList();
			Current.PopulateItems();

			Game.Hud.FindPopupPanel().AddChild( Current );
		}

		public void PopulateItems()
		{
			Container.DeleteChildren();

			var button = Container.Add.Button( "Close" );
			button.AddClass( "editor-button" );
			button.AddClass( "secondary" );
			button.AddEventListener( "onclick", () => Delete() );

			var attributes = Library.GetAttributes<EditorEntityAttribute>().ToList();

			for ( int i = 0; i < attributes.Count; i++ )
			{
				var attribute = attributes[i];
				button = Container.Add.Button( attribute.Name );
				button.AddClass( "editor-button" );
				button.AddEventListener( "onclick", () => OnItemSelected( attribute ) );
			}
		}

		protected virtual void OnItemSelected( EditorEntityAttribute attribute )
		{
			EntitiesTool.ChangeLibraryAttributeCmd( attribute.Name );
			Delete();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
