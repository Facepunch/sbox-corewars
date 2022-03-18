using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	public interface IDroppable
	{
		void RemoveClass( string className );
		void AddClass( string className );
		bool CanDrop( IDraggable draggable );
		void OnDrop( IDraggable draggable );
	}
}
