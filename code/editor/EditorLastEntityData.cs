using Sandbox;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorLastEntityData : BaseNetworkable
	{
		[Net] public IDictionary<string, object> EditorData { get; set; }
		[Net, Change] public string TypeName { get; set; }

		public Dictionary<string, object> Properties { get; set; } = new();
		public Type Type { get; set; }

		public EditorLastEntityData()
		{
			EditorData = new Dictionary<string, object>();
		}

		public bool IsSameType( TypeDescription description )
		{
			if ( Type == null ) return false;
			return description.ClassName == Type.Name;
		}

		public bool IsSameType( Type type )
		{
			return type == Type;
		}

		public bool IsSameType( Entity entity )
		{
			return entity.GetType() == Type;
		}

		public void SetEditorData( string key, object value )
		{
			EditorData[key] = value;
		}

		public void StoreProperty( string key, object value )
		{
			Properties[key] = value;
		}

		public T GetEditorData<T>( string key )
		{
			if ( EditorData.TryGetValue( key, out var value ) )
			{
				return (T)value;
			}

			return default;
		}

		public bool TryCopyTo( Entity entity )
		{
			if ( !IsSameType( entity ) ) return false;

			var properties = TypeLibrary.GetProperties( entity );

			foreach ( var property in properties )
			{
				if ( property.GetCustomAttribute<EditorPropertyAttribute>() == null )
				{
					continue;
				}

				if ( Properties.TryGetValue( property.Name, out var value ) )
				{
					property.SetValue( entity, value );

					if ( entity is IEditorCallbacks callbacks )
					{
						callbacks.OnPropertyChanged( property.Name );
					}
				}
			}

			return true;
		}

		public void SetEntity( Entity entity )
		{
			var type = entity.GetType();
			Properties.Clear();
			TypeName = type.Name;
			Type = type;
		}

		protected void OnTypeNameChanged( string typeName )
		{
			Type = TypeLibrary.GetTypeByName( typeName );
		}
	}
}
