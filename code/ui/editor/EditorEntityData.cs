using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorEntityData : Panel
	{
		private static object ConvertPropertyValue( PropertyInfo property, string value )
		{
			object convertedValue;

			if ( property.PropertyType.IsEnum )
				convertedValue = Enum.Parse( property.PropertyType, value );
			else if ( property.PropertyType == typeof( float ) )
				convertedValue = Convert.ToSingle( value );
			else if ( property.PropertyType == typeof( int ) )
				convertedValue = Convert.ToInt32( value );
			else if ( property.PropertyType == typeof( bool ) )
				convertedValue = Convert.ToBoolean( value );
			else
				convertedValue = value;

			return convertedValue;
		}

		[ServerCmd]
		public static void SaveEntityKeyValue( int entityId, string key, string value )
		{
			var entity = Sandbox.Entity.FindByIndex( entityId );
			if ( !entity.IsValid() ) return;

			var properties = Reflection.GetProperties( entity );

			foreach ( var property in properties )
			{
				if ( property.Name != key ) continue;
				if ( property.GetCustomAttribute<PropertyAttribute>() == null ) continue;

				property.SetValue( entity, ConvertPropertyValue( property, value ) );
			}
		}

		[ServerCmd( "cw_open_entity_data" )]
		public static void SendOpenRequest( int entityId )
		{
			var entity = Sandbox.Entity.FindByIndex( entityId );
			if ( !entity.IsValid() ) return;

			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					var properties = Reflection.GetProperties( entity )
						.Where( property => property.GetCustomAttribute<PropertyAttribute>() != null );

					writer.Write( properties.Count() );

					foreach ( var property in properties )
					{
						var value = property.GetValue( entity );
						writer.Write( property.Name );
						writer.Write( value.ToString() );
					}
				}

				OpenWithValues( To.Single( ConsoleSystem.Caller ), entityId, stream.ToArray() );
			}
		}

		[ClientRpc]
		public static void OpenWithValues( int entityId, byte[] data )
		{
			var entity = Sandbox.Entity.FindByIndex( entityId );
			if ( !entity.IsValid() ) return;

			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					var propertyCount = reader.ReadInt32();
					var properties = Reflection.GetProperties( entity );

					for ( var i = 0; i < propertyCount; i++ )
					{
						var name = reader.ReadString();
						var value = reader.ReadString();
						var property = properties.FirstOrDefault( p => p.Name == name );
						property.SetValue( entity, ConvertPropertyValue( property, value ) );
					}
				}
			}

			Open( (ISourceEntity)entity );
		}

		public static EditorEntityData Current { get; private set; }

		public SimpleForm PropertyForm { get; set; }
		public string Title => Entity.GetType().Name;
		public ISourceEntity Entity { get; private set; }

		private Dictionary<string, object> ChangedValues { get; set; } = new();

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

			ChangedValues.Clear();

			var properties = Reflection.GetProperties( Entity );

			for ( int i = 0; i < properties.Length; i++ )
			{
				var property = properties[i];

				if ( property.GetCustomAttribute<PropertyAttribute>() == null )
					continue;

				PropertyForm.AddRowWithCallback( property, Entity, PropertyForm.CreateControlFor( property ), ( value ) =>
				{
					ChangedValues[property.Name] = value;
				} );
			}

			PropertyForm.EndGroup();

			var button = PropertyForm.Add.Button( "Save" );
			button.AddClass( "editor-button" );
			button.AddEventListener( "onclick", () => Save() );
		}

		protected virtual void Save()
		{
			foreach ( var kv in ChangedValues )
			{
				SaveEntityKeyValue( Entity.NetworkIdent, kv.Key, kv.Value.ToString() );
			}

			ChangedValues.Clear();

			Delete();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
