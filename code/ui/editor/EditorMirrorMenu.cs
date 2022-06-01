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
		public SliderEntry XOriginOffset { get; set; }
		public Checkbox YAxis { get; set; }
		public SliderEntry YOriginOffset { get; set; }

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

			XOriginOffset = new SliderEntry();
			XOriginOffset.MinValue = -10f;
			XOriginOffset.MaxValue = 10f;
			XOriginOffset.Value = 0f;
			XOriginOffset.Step = 1f;

			YOriginOffset = new SliderEntry();
			YOriginOffset.MinValue = -10f;
			YOriginOffset.MaxValue = 10f;
			YOriginOffset.Value = 0f;
			YOriginOffset.Step = 1f;

			FromOrigin = new Checkbox();
			XAxis = new Checkbox();
			YAxis = new Checkbox();

			PropertyForm.AddRowToGroup( "From Origin", FromOrigin );
			PropertyForm.AddRowToGroup( "X Axis", XAxis );
			PropertyForm.AddRowToGroup( "X Origin Offset", XOriginOffset );
			PropertyForm.AddRowToGroup( "Y Axis", YAxis );
			PropertyForm.AddRowToGroup( "Y Origin Offset", YOriginOffset );

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
					tool.Mirror( XAxis.Checked, YAxis.Checked, FromOrigin.Checked, XOriginOffset.Value.CeilToInt(), YOriginOffset.Value.CeilToInt() );
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
