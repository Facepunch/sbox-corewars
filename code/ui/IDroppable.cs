using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	public interface IDroppable
	{
		void RemoveClass( string className );
		void AddClass( string className );
		bool CanDrop( IDraggable draggable, DraggableMode mode );
		void OnDrop( IDraggable draggable, DraggableMode mode );
	}
}
