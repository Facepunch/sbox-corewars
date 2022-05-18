using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public enum RoundStage
	{
		None,
		Start,
		GoldII,
		CrystalII,
		GoldIII,
		CrystalIII,
		NoBeds,
		SuddenDeath,
		End
	}

	public static class RoundStageExtensions
	{
		public static RoundStage GetNextStage( this RoundStage stage )
		{
			if ( stage + 1 > RoundStage.End )
				return RoundStage.None;
			else
				return stage + 1;
		}

		public static string GetName( this RoundStage stage )
		{
			switch ( stage )
			{
				case RoundStage.Start:
					return "Start";
				case RoundStage.GoldII:
					return "Gold II";
				case RoundStage.CrystalII:
					return "Crystal II";
				case RoundStage.GoldIII:
					return "Gold III";
				case RoundStage.CrystalIII:
					return "Crystal III";
				case RoundStage.NoBeds:
					return "No Beds";
				case RoundStage.SuddenDeath:
					return "Sudden Death";
				case RoundStage.End:
					return "Game Over";
			}

			return string.Empty;
		}
	}
}
