using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars.Editor
{
	public interface IAutoCompleteList
	{
		void ClearOptions();
		void AddOption( string option, Action callback );
	}

	public class AutoCompleteInput : TextEntry
	{
		public IAutoCompleteList AutoCompleteList { get; set; }
		public Func<string, string[]> AutoCompleteHandler { get; set; }

		public void SetAutoCompleteList( IAutoCompleteList list )
		{
			AutoCompleteList = list;
		}

		public override void OnValueChanged()
		{
			base.OnValueChanged();

			UpdateAutoCompleteList();
		}

		public void UpdateAutoCompleteList()
		{
			UpdateAutoCompleteList( Text );
		}

		public void UpdateAutoCompleteList( string partial )
		{
			var results = AutoCompleteHandler?.Invoke( partial );

			AutoCompleteList.ClearOptions();

			if ( results == null ) return;

			for ( int i = 0; i < results.Length; i++ )
			{
				var result = results[i];
				AutoCompleteList.AddOption( result, () =>
				{
					Text = result;
					CaretPosition = TextLength;
					UpdateAutoCompleteList();
				} );
			}
		}
	}
}
