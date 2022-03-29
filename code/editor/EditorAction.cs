using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	public abstract class EditorAction
	{
		public static Dictionary<int, object> ObjectLookup { get; private set; } = new();
		public static int NextObjectId { get; private set; }

		public static int AddObject( object item )
		{
			var id = NextObjectId++;
			ObjectLookup[id] = item;
			return id;
		}

		public static bool FindObject<T>( int id, out T item ) where T : class
		{
			if ( ObjectLookup.TryGetValue( id, out var found ) )
			{
				if ( found is T )
				{
					item = (found as T);
					return true;
				}
			}

			item = default;
			return false;
		}

		public static bool FindObjectId( object item, out int id )
		{
			foreach ( var kv in ObjectLookup )
			{
				if ( kv.Value == item )
				{
					id = kv.Key;
					return true;
				}
			}

			id = 0;
			return false;
		}

		public static void UpdateObject( int id, object item )
		{
			ObjectLookup[id] = item;
		}

		public virtual string Name => "Action";

		public virtual void Undo()
		{

		}

		public virtual void Perform()
		{

		}
	}
}
