using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public partial class GameState : BaseState
	{
		public Dictionary<long,Team> PlayerToTeam { get; set; } = new();

		[Net] public RealTimeUntil NextStageTime { get; private set; }
		[Net, Change( nameof( OnStageChanged ) )] public RoundStage Stage { get; private set; }

		public bool HasReachedStage( RoundStage stage )
		{
			return Stage >= stage;
		}

		public override void OnEnter()
		{
			if ( IsServer )
			{
				IResettable.ResetAll();

				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.AssignRandomTeam( true );
					player.RespawnWhenAvailable();

					var playerId = player.Client.PlayerId;
					PlayerToTeam[playerId] = player.Team;
				}

				NextStageTime = 300f;
				Stage = RoundStage.Start;
			}
		}

		public override bool CanHearPlayerVoice( Client a, Client b )
		{
			if ( a.Pawn is not Player source )
				return false;

			if ( b.Pawn is not Player destination )
				return false;

			return source.Team == destination.Team;
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			var playerId = player.Client.PlayerId;

			if ( !PlayerToTeam.ContainsKey( playerId ) )
				player.AssignRandomTeam( true );
			else
				player.SetTeam( PlayerToTeam[playerId] );

			PlayerToTeam[playerId] = player.Team;

			player.RespawnWhenAvailable();
		}

		protected virtual void OnStageChanged( RoundStage stage )
		{
			Log.Info( $"Stage changed to {stage}" );
		}

		protected virtual void ServerTick()
		{
			if ( !NextStageTime ) return;

			if ( Stage == RoundStage.Start )
			{
				NextStageTime = 300f;
				Stage = RoundStage.GoldII;
			}
			else if ( Stage == RoundStage.GoldII )
			{
				NextStageTime = 300f;
				Stage = RoundStage.CrystalII;
			}
			else if ( Stage == RoundStage.CrystalII )
			{
				NextStageTime = 300f;
				Stage = RoundStage.GoldIII;
			}
			else if ( Stage == RoundStage.GoldIII )
			{
				NextStageTime = 300f;
				Stage = RoundStage.CrystalIII;
			}
			else if ( Stage == RoundStage.CrystalIII )
			{
				NextStageTime = 600f;
				Stage = RoundStage.NoBeds;
			}
			else if ( Stage == RoundStage.NoBeds )
			{
				NextStageTime = 300f;
				Stage = RoundStage.SuddenDeath;
			}
			else if ( Stage == RoundStage.SuddenDeath )
			{
				NextStageTime = 5f;
				Stage = RoundStage.End;
			}
			else
			{
				System.Set( new SummaryState() );
			}
		}
	}
}
