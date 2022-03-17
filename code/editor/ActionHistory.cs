using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	public class ActionHistory<T>
	{
		private LinkedList<T> InternalItems = new LinkedList<T>();

		public int Capacity { get; }

		public ActionHistory( int capacity )
		{
			Capacity = capacity;
		}

		public void Push( T item )
		{
			if ( InternalItems.Count == Capacity )
			{
				InternalItems.RemoveFirst();
				InternalItems.AddLast( item );
			}
			else
			{
				InternalItems.AddLast( new LinkedListNode<T>( item ) );
			}
		}

		public bool TryPop( out T item )
		{
			if ( InternalItems.Count == 0 )
			{
				item = default;
				return false;
			}

			item = Pop();
			return true;
		}

		public T Pop()
		{
			if ( InternalItems.Count == 0 )
			{
				return default;
			}

			var node = InternalItems.Last;
			InternalItems.RemoveLast();
			return node == null ? default : node.Value;
		}
	}
}
