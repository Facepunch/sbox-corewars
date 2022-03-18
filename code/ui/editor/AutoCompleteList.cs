using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public class AutoCompleteList : Panel, IAutoCompleteList
	{
		public void ClearOptions()
		{
			DeleteChildren();
		}

		public void AddOption( string option, Action callback )
		{
			var item = AddChild<Button>( "option" );
			item.AddEventListener( "onclick", callback );
			item.SetText( option );
		}
	}
}
