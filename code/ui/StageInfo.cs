using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class StageInfo : Panel
	{
		public string NextStageText => GetNextStageText();
		public string StageText => GetStageText();
		public string TimeText => GetTimeText();

		protected override void PostTemplateApplied()
		{
			BindClass( "hidden", IsHidden );
			base.PostTemplateApplied();
		}

		private string GetNextStageText()
		{
			if ( Game.TryGetState<GameState>( out var state ) )
			{
				return state.Stage.GetNextStage().GetName();
			}

			return string.Empty;
		}

		private string GetStageText()
		{
			if ( Game.TryGetState<GameState>( out var state ) )
			{
				return state.Stage.GetName();
			}

			return string.Empty;
		}

		private string GetTimeText()
		{
			if ( Game.TryGetState<GameState>( out var state ) )
			{
				return TimeSpan.FromSeconds( state.NextStageTime.Absolute ).ToString( @"mm\:ss" );
			}

			return string.Empty;
		}

		private bool IsHidden()
		{
			if ( IDialog.IsActive() || !Game.IsState<GameState>() )
				return true;

			return false;
		}
	}
}
