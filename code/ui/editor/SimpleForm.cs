﻿using Sandbox.UI;
using System.Linq;
using System.Reflection;
using Sandbox;
using System;
using Sandbox.UI.Construct;

namespace Facepunch.CoreWars.Editor
{
	public class SimpleForm : Form
	{
		public SimpleForm() : base()
		{

		}

		public void AddRowToGroup( string entryTitle, Panel control )
		{
			var row = (currentGroup ?? this).AddChild<Field>();

			var title = row.Add.Panel( "label" );
			title.Add.Label( entryTitle );

			var value = row.AddChild<FieldControl>();
			control.Parent = value;
		}

		public void AddRowWithCallback( PropertyDescription member, object target, Panel control, Action<object> callback )
		{
			var entryTitle = member.Name;
			var row = (currentGroup ?? this).AddChild<Field>();

			var title = row.Add.Panel( "label" );
			title.Add.Label( entryTitle );

			var value = row.AddChild<FieldControl>();
			control.Parent = value;
			control.SetPropertyObject( "value", member.GetValue( target ) );
			control.SetClass( "disabled", !member.CanWrite );
			control.AddEventListener( "value.changed", ( e ) =>
			{
				callback?.Invoke( e.Value );
			} );
		}

		public Panel CreateControlFor( PropertyDescription property )
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
					var slider = new SliderControl
					{
						Min = range.Min,
						Max = range.Max,
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
					var slider = new SliderControl
					{
						Min = range.Min,
						Max = range.Max,
						Step = range.Step
					};

					if ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( uint ) )
					{
						slider.NumberFormat = "0.";
						slider.Step = 1f;
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

		public void StartGroup()
		{
			currentGroup = Add.Panel( "field-group" );
		}

		public void EndGroup()
		{
			currentGroup = null;
		}
	}
}
