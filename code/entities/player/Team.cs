using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	public enum Team
	{
		None,
		Blue,
		Red,
		Orange,
		Green,
		Purple,
		Pink,
		Cyan,
		Yellow
	}

	public static class TeamExtensions
	{
		private static Dictionary<Team, Color> ColorMap = new()
		{
			{ Team.None, Color.White },
			{ Team.Blue, Util.HexToColor( "#59adf6") },
			{ Team.Red, Util.HexToColor( "#ff6961") },
			{ Team.Orange, Util.HexToColor( "#ffb480") },
			{ Team.Green, Util.HexToColor( "#42d6a4") },
			{ Team.Purple, Util.HexToColor( "#9d94ff") },
			{ Team.Pink, Util.HexToColor( "#c780e8") },
			{ Team.Cyan, Util.HexToColor( "#08cad1") },
			{ Team.Yellow, Util.HexToColor( "#f8f38d") }
		};

		private static Dictionary<Team, TeamCore> Cores = new();

		public static BlockType GetFungusBlock( this Team team )
		{
			var world = VoxelWorld.Current;
			if ( !world.IsValid() ) return null;

			var blockId = team switch
			{
				Team.Blue => world.FindBlockId<BlueFungusBlock>(),
				Team.Red => world.FindBlockId<RedFungusBlock>(),
				Team.Orange => world.FindBlockId<OrangeFungusBlock>(),
				Team.Green => world.FindBlockId<GreenFungusBlock>(),
				Team.Purple => world.FindBlockId<PurpleFungusBlock>(),
				Team.Pink => world.FindBlockId<PinkFungusBlock>(),
				Team.Cyan => world.FindBlockId<CyanFungusBlock>(),
				Team.Yellow => world.FindBlockId<YellowFungusBlock>(),
				_ => throw new System.NotImplementedException()
			};

			return world.GetBlockType( blockId );
		}

		public static To GetTo( this Team team )
		{
			var players = team.GetPlayers();
			return To.Multiple( players.Select( p => p.Client ) );
		}

		public static string GetHudClass( this Team team )
		{
			return team switch
			{
				Team.None => "team_none",
				Team.Blue => "team_blue",
				Team.Red => "team_red",
				Team.Orange => "team_orange",
				Team.Green => "team_green",
				Team.Purple => "team_purple",
				Team.Pink => "team_pink",
				Team.Cyan => "team_cyan",
				Team.Yellow => "team_yellow",
				_ => throw new System.NotImplementedException()
			};
		}

		public static TeamCore GetCore( this Team team )
		{
			if ( !Cores.TryGetValue( team, out var core ) )
			{
				core = Entity.All.OfType<TeamCore>().FirstOrDefault( t => t.Team == team );
				Cores[team] = core;
			}

			return core;
		}

		public static IEnumerable<Player> GetPlayers( this Team team )
		{
			foreach ( var player in Entity.All.OfType<Player>() )
			{
				if ( player.Team == team )
				{
					yield return player;
				}
			}
		}

		public static Color GetColor( this Team team )
		{
			if ( ColorMap.TryGetValue( team, out var color ) )
			{
				return color;
			}

			return default;
		}

		public static BlockItem CreateFungusBlockItem( this Team team )
		{
			var block = team.GetFungusBlock();
			if ( block == null ) return null;

			var item = InventorySystem.CreateItem<BlockItem>();
			item.BlockId = block.BlockId;

			return item;
		}
	}
}
