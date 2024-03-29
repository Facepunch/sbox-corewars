﻿
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars.UI
{
	public partial class TabTextEntry : TextEntry
	{
		public event Action OnTabPressed;
		
		public bool IsShiftDown { get; private set; }

		public override void OnButtonTyped( string button, KeyModifiers km )
		{
			if ( button == "tab" )
			{
				OnTabPressed?.Invoke();
				return;
			}

			base.OnButtonTyped( button, km );
		}

		public override void OnButtonEvent( ButtonEvent e )
		{
			if ( e.Button == "lshift" )
			{
				IsShiftDown = e.Pressed;
			}

			base.OnButtonEvent( e );
		}

		protected override void OnBlur( PanelEvent e )
		{
			IsShiftDown = false;
			base.OnBlur( e );
		}
	}
}
