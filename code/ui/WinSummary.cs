﻿using Sandbox.UI;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class WinSummary : Panel, IDialog
	{
		public static WinSummary Current { get; private set; }

		public bool IsOpen { get; set; }

		public WinSummary()
		{
			Current = this;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			PlaySound( "itemstore.open" );
			IDialog.Activate( this );
			IsOpen = true;
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			IDialog.Deactivate( this );
			IsOpen = false;
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			BindClass( "hidden", () => !IsOpen );
		}
	}
}
