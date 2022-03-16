using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorLoadDialogItem : Button
	{
		public string FileName { get; set; }
		public Action OnSelect { get; set; }

		protected override void OnClick( MousePanelEvent e )
		{
			OnSelect?.Invoke();

			base.OnClick( e );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}
	}
}
