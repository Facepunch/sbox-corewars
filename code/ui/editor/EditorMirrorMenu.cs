using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorMirrorMenu : Panel
	{
		public static EditorMirrorMenu Current { get; private set; }

		public SimpleForm PropertyForm { get; set; }
		public Checkbox FromOrigin { get; set; }
		public Checkbox XAxis { get; set; }
		public Checkbox YAxis { get; set; }

		public static void Open()
		{
			Current?.Delete();
			Current = new EditorMirrorMenu();
			Current.PopulateItems();

			Game.Hud.FindPopupPanel().AddChild( Current );
		}

		public void PopulateItems()
		{
			PropertyForm.Clear();
			PropertyForm.StartGroup();

			FromOrigin = new Checkbox();
			XAxis = new Checkbox();
			YAxis = new Checkbox();

			PropertyForm.AddRowToGroup( "From Origin", FromOrigin );
			PropertyForm.AddRowToGroup( "X Axis", XAxis );
			PropertyForm.AddRowToGroup( "Y Axis", YAxis );

			PropertyForm.EndGroup();

			var mirrorBtn = PropertyForm.Add.Button( "Mirror", "mirrorBtn" );
			mirrorBtn.AddClass( "editor-button" );
			mirrorBtn.AddEventListener( "onclick", () => Save() );

			var cancelBtn = PropertyForm.Add.Button( "Cancel", "cancelBtn" );
			cancelBtn.AddClass( "editor-button secondary" );
			cancelBtn.AddEventListener( "onclick", () => Cancel() );
		}

		protected virtual void Save()
		{
			if ( Local.Pawn is EditorPlayer player )
			{
				if ( player.Tool is MirrorBlocksTool tool )
				{
					tool.Mirror( XAxis.Checked, YAxis.Checked, FromOrigin.Checked );
				}
			}

			Delete();
			Current = null;
		}

		protected virtual void Cancel()
		{
			if ( Local.Pawn is EditorPlayer player )
			{
				if ( player.Tool is MirrorBlocksTool tool )
				{
					tool.Cancel();
				}
			}

			Delete();
			Current = null;
		}
	}
}
