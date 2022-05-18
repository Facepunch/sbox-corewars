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
		NoCores,
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

		public static string GetIcon( this RoundStage stage )
		{
			switch ( stage )
			{
				case RoundStage.Start:
					return "textures/ui/logo_spinner.png";
				case RoundStage.GoldII:
					return "textures/items/gold.png";
				case RoundStage.CrystalII:
					return "textures/items/crystal.png";
				case RoundStage.GoldIII:
					return "textures/items/gold.png";
				case RoundStage.CrystalIII:
					return "textures/items/crystal.png";
				case RoundStage.NoCores:
					return "textures/ui/no_core.png";
				case RoundStage.SuddenDeath:
					return "textures/ui/skull.png";
				case RoundStage.End:
					return "textures/ui/skull.png";
			}

			return string.Empty;
		}

		public static string GetName( this RoundStage stage )
		{
			switch ( stage )
			{
				case RoundStage.Start:
					return "Core Wars";
				case RoundStage.GoldII:
					return "Gold II";
				case RoundStage.CrystalII:
					return "Crystal II";
				case RoundStage.GoldIII:
					return "Gold III";
				case RoundStage.CrystalIII:
					return "Crystal III";
				case RoundStage.NoCores:
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
