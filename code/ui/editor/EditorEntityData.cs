using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Reflection;

namespace Facepunch.CoreWars.Editor
{
	public class EditorEntityDataForm : Form
	{
		public EditorEntityDataForm() : base() {

		}

		public void StartGroup()
		{
			currentGroup = Add.Panel( "field-group" );
		}

		public void EndGroup()
		{
			currentGroup = null;
		}
	}

	[UseTemplate]
	public partial class EditorEntityData : Panel
	{
		public static EditorEntityData Current { get; private set; }

		public EditorEntityDataForm PropertyForm { get; set; }
		public string Title => Entity.GetType().Name;
		public ISourceEntity Entity { get; private set; }

		public static void Open( ISourceEntity entity )
		{
			Current?.Delete();
			Current = new EditorEntityData( entity );
			Current.PopulateItems();

			Game.Hud.FindPopupPanel().AddChild( Current );
		}

		public EditorEntityData( ISourceEntity entity )
		{
			Entity = entity;
		}

		public void PopulateItems()
		{
			PropertyForm.Clear();
			PropertyForm.StartGroup();

			foreach ( var p in Reflection.GetProperties( Entity ) )
			{
				if ( p.GetCustomAttribute<EditorEntityPropertyAttribute>() != null )
				{
					PropertyForm.AddRow( p, Entity, CreateControlFor( p ) );
				}
			}

			PropertyForm.EndGroup();

			var button = PropertyForm.Add.Button( "Save" );
			button.AddClass( "editor-button" );
			button.AddEventListener( "onclick", () => Delete() );
		}

		private Panel CreateControlFor( PropertyInfo property )
		{
			if ( property.PropertyType.IsEnum )
			{
				var control = new DropDown();
				var names = property.PropertyType.GetEnumNames();
				var values = property.PropertyType.GetEnumValues();

				for ( int i = 0; i < names.Length; i++ )
				{
					control.Options.Add( new Option( names[i], values.GetValue( i ).ToString() ) );
				}

				return control;
			}
			else if ( property.PropertyType == typeof( float ) )
			{
				var range = property.GetCustomAttribute<RangeAttribute>();

				if ( range != null )
				{
					var slider = new SliderEntry
					{
						MinValue = range.Min,
						MaxValue = range.Max,
						Step = range.Step
					};
					return slider;
				}

				var control = new TextEntry
				{
					Numeric = true,
					NumberFormat = "0.###"
				};

				return control;
			}
			else if ( property.PropertyType == typeof( int ) )
			{
				var range = property.GetCustomAttribute<RangeAttribute>();

				if ( range != null )
				{
					var slider = new SliderEntry
					{
						MinValue = range.Min,
						MaxValue = range.Max,
						Step = range.Step
					};

					if ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( uint ) )
					{
						slider.TextEntry.NumberFormat = "0.";
						slider.Slider.Step = 1;
					}

					return slider;
				}

				var control = new TextEntry();
				control.Numeric = true;
				control.NumberFormat = "0";
				return control;
			}
			else if ( property.PropertyType == typeof( bool ) )
			{
				return new Checkbox();
			}
			else
			{
				return new TextEntry();
			}
		}

		protected virtual void OnItemSelected( EditorEntityItem item )
		{
			EntitiesTool.ChangeLibraryAttributeCmd( item.Attribute.Name );
			Delete();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
