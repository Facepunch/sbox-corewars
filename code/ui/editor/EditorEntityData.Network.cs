using Sandbox;
using System.IO;
using System.Linq;
using Facepunch.Voxels;

namespace Facepunch.CoreWars.Editor;

public partial class EditorEntityData
{
	[ConCmd.Server]
	public static void SaveEntityKeyValue( int entityId, string key, string value )
	{
		if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
			return;

		var entity = Sandbox.Entity.FindByIndex( entityId );
		if ( !entity.IsValid() ) return;

		if ( !player.LastPlacedEntity.IsSameType( entity ) )
		{
			player.LastPlacedEntity.SetEntity( entity );
		}

		var properties = TypeLibrary.GetPropertyDescriptions( entity );
		var callbacks = (entity as IEditorCallbacks);

		foreach ( var property in properties )
		{
			if ( property.GetCustomAttribute<EditorPropertyAttribute>() == null )
			{
				continue;
			}

			if ( property.Name != key )
			{
				player.LastPlacedEntity.StoreProperty( property.Name, property.GetValue( entity ) );
				continue;
			}

			property.SetValue( entity, ConvertPropertyValue( property, value ) );

			callbacks?.OnPropertyChanged( key );

			player.LastPlacedEntity.StoreProperty( property.Name, property.GetValue( entity ) );
		}

		callbacks?.OnPlayerSavedData( player );
	}

	[ConCmd.Server( "cw_open_entity_data" )]
	public static void SendOpenRequest( int entityId )
	{
		var entity = Sandbox.Entity.FindByIndex( entityId );
		if ( !entity.IsValid() ) return;

		using ( var stream = new MemoryStream() )
		{
			using ( var writer = new BinaryWriter( stream ) )
			{
				var properties = TypeLibrary.GetPropertyDescriptions( entity )
					.Where( property => property.GetCustomAttribute<EditorPropertyAttribute>() != null );

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
				var properties = TypeLibrary.GetPropertyDescriptions( entity );

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
}
