using System.IO;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public interface IEditorCallbacks
	{
		void OnPropertyChanged( string propertyName );
		void OnPlayerSavedData( EditorPlayer player );
	}
}

