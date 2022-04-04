﻿using Facepunch.Voxels;
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
	public partial class EditorBlockData : Panel
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
		public static void SaveBlockDataValue( int x, int y, int z, string key, string value )
		{
			var world = VoxelWorld.Current;
			if ( !world.IsValid() ) return;

			var voxel = world.GetVoxel( x, y, z );
			var state = world.GetOrCreateState<BlockState>( voxel.Position );

			var properties = Reflection.GetProperties( state );

			foreach ( var property in properties )
			{
				if ( property.Name != key ) continue;
				if ( property.GetCustomAttribute<PropertyAttribute>() == null ) continue;
				property.SetValue( state, ConvertPropertyValue( property, value ) );
			}

			state.IsDirty = true;
		}

		[ServerCmd( "cw_open_block_data" )]
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
					var properties = Reflection.GetProperties( state )
						.Where( property => property.GetCustomAttribute<PropertyAttribute>() != null );

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
					var properties = Reflection.GetProperties( state );

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

		public static EditorBlockData Current { get; private set; }

		public SimpleForm PropertyForm { get; set; }
		public string Title => Voxel.GetBlockType().FriendlyName;
		public BlockState State { get; private set; }
		public Voxel Voxel { get; private set; }

		private Dictionary<string, object> ChangedValues { get; set; } = new();

		public static void Open( Voxel voxel, BlockState state )
		{
			Current?.Delete();
			Current = new EditorBlockData( voxel, state );
			Current.PopulateItems();

			Game.Hud.FindPopupPanel().AddChild( Current );
		}

		public EditorBlockData( Voxel voxel, BlockState state )
		{
			Voxel = voxel;
			State = state;
		}

		public void PopulateItems()
		{
			PropertyForm.Clear();
			PropertyForm.StartGroup();

			ChangedValues.Clear();

			var properties = Reflection.GetProperties( State );

			for ( int i = 0; i < properties.Length; i++ )
			{
				var property = properties[i];

				if ( property.GetCustomAttribute<PropertyAttribute>() == null )
					continue;

				PropertyForm.AddRowWithCallback( property, State, PropertyForm.CreateControlFor( property ), ( value ) =>
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
				SaveBlockDataValue( Voxel.Position.x, Voxel.Position.y, Voxel.Position.z, kv.Key, kv.Value.ToString() );
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