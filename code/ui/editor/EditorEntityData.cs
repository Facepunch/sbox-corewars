using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorEntityData : Panel
	{
		public static EditorEntityData Current { get; private set; }

		public string Title => Entity.GetType().Name;
		public ISourceEntity Entity { get; private set; }
		public Panel Items { get; set; }

		public static void Open( ISourceEntity entity )
		{
			Current?.Delete();
			Current = new EditorEntityData( entity );
			Current.PopulateItems();

			Game.Hud.AddChild( Current );
		}

		public EditorEntityData( ISourceEntity entity )
		{
			AcceptsFocus = true;
			Entity = entity;
			Focus();
		}

		public void PopulateItems()
		{
			Items.DeleteChildren();

			/*
			var attributes = Library.GetAttributes<EditorEntityLibraryAttribute>();

			foreach ( var attribute in attributes )
			{
				var item = Items.AddChild<EditorEntityItem>();
				item.SetAttribute( attribute );
				item.OnSelected = () => OnItemSelected( item );
			}
			*/
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
