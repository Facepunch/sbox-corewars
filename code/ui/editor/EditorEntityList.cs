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

		public void Close()
		{
			Delete();
		}

		public void PopulateItems()
		{
			Container.DeleteChildren();

			var descriptions = TypeLibrary.GetDescriptions<Entity>().Where( d => d.GetAttribute<EditorEntityAttribute>() != null ).ToList();
			var categories = new Dictionary<string, List<TypeDescription>>();

			descriptions.Sort( ( a, b ) => a.Group.CompareTo( b.Group ) );

			for ( int i = 0; i < descriptions.Count; i++ )
			{
				var description = descriptions[i];
				var group = string.IsNullOrEmpty( description.Group ) ? "Other" : description.Group;
				
				if ( !categories.TryGetValue( group, out var list ) )
				{
					list = new List<TypeDescription>();
					categories.Add( group, list );
				}

				list.Add( description );
			}

			foreach ( var kv in categories )
			{
				var category = kv.Key;
				var values = kv.Value;
				var container = Container.Add.Panel( "category" );
				var label = container.Add.Label( category, "title" );

				for ( int i = 0; i < values.Count; i++ )
				{
					var description = values[i];
					var button = container.Add.Button( description.Title );
					button.AddClass( "editor-button" );
					button.AddEventListener( "onclick", () => OnItemSelected( description ) );
				}
			}
		}

		protected virtual void OnItemSelected( TypeDescription description )
		{
			EntitiesTool.ChangeEntityToolCmd( description.ClassName );
			Delete();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
