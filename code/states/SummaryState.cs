using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class SummaryState : BaseState
	{
		[Net] public RealTimeUntil NextStageTime { get; private set; }
		[Net] public Team WinningTeam { get; private set; } = Team.None;

		public override void OnEnter()
		{
			if ( IsServer )
			{
				NextStageTime = 10f;
				CalculateWinningTeam();
			}
			else
			{
				WinSummary.Current.Open();
				WinSummary.Current.Populate( NextStageTime, WinningTeam );
			}
		}

		public override void OnLeave()
		{
			if ( IsClient )
			{
				WinSummary.Current.Close();
			}

			base.OnLeave();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( NextStageTime )
			{
				System.Set( new LobbyState() );
			}
		}

		private void CalculateWinningTeam()
		{
			var core = Entity.All.OfType<TeamCore>().Where( c => c.LifeState == LifeState.Alive ).FirstOrDefault();

			if ( core.IsValid() )
			{
				WinningTeam = core.Team;
				return;
			}

			var team = Entity.All.OfType<Player>()
				.Where( p => p.LifeState == LifeState.Alive )
				.Select( p => p.Team )
				.GroupBy( i => i )
				.OrderByDescending( g => g.Count() )
				.Select( g => g.Key )
				.FirstOrDefault();

			WinningTeam = team;
		}
	}
}
