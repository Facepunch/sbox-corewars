using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Facepunch.Voxels;
using System.IO;

namespace Facepunch.CoreWars.Editor;

public partial class EditorBlockData
{
	[ConCmd.Server]
	public static void SaveBlockDataValue( int x, int y, int z, string key, string value )
	{
		var world = VoxelWorld.Current;
		if ( !world.IsValid() ) return;

		var voxel = world.GetVoxel( x, y, z );
		var state = world.GetOrCreateState<BlockState>( voxel.Position );
		var properties = TypeLibrary.GetPropertyDescriptions( state );

		foreach ( var property in properties )
		{
			if ( property.Name != key ) continue;
			if ( property.GetCustomAttribute<EditorPropertyAttribute>() == null ) continue;
			property.SetValue( state, ConvertPropertyValue( property, value ) );
		}

		state.IsDirty = true;
	}

	[ConCmd.Server( "cw_open_block_data" )]
	public static void SendOpenRequest( int x, int y, int z )
	{
		var world = VoxelWorld.Current;
		if ( !world.IsValid() ) return;

		var voxel = world.GetVoxel( x, y, z );
		var state = world.GetOrCreateState<BlockState>( voxel.Position );

		using ( var stream = new MemoryStream() )
		{
			using ( var writer = new BinaryWriter( stream ) )
			{
				var properties = TypeLibrary.GetPropertyDescriptions( state )
					.Where( property => property.GetCustomAttribute<EditorPropertyAttribute>() != null );

				writer.Write( properties.Count() );

				foreach ( var property in properties )
				{
					var value = property.GetValue( state );
					writer.Write( property.Name );
					writer.Write( value.ToString() );
				}
			}

			OpenWithValues( To.Single( ConsoleSystem.Caller ), x, y, z, stream.ToArray() );
		}
	}

	[ClientRpc]
	public static void OpenWithValues( int x, int y, int z, byte[] data )
	{
		var world = VoxelWorld.Current;
		if ( !world.IsValid() ) return;

		var voxel = world.GetVoxel( x, y, z );
		var state = world.GetOrCreateState<BlockState>( voxel.Position );

		using ( var stream = new MemoryStream( data ) )
		{
			using ( var reader = new BinaryReader( stream ) )
			{
				var propertyCount = reader.ReadInt32();
				var properties = TypeLibrary.GetPropertyDescriptions( state );

				for ( var i = 0; i < propertyCount; i++ )
				{
					var name = reader.ReadString();
					var value = reader.ReadString();
					var property = properties.FirstOrDefault( p => p.Name == name );
					property.SetValue( state, ConvertPropertyValue( property, value ) );
				}
			}
		}

		Open( voxel, state );
	}
}
