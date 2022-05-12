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

		public float StageTimeMultiplier => 1f;

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

				Announcements.Send( To.Everyone, "Core Wars", "Destroy your opponents Core to prevent them respawning!", "textures/ui/logo_spinner.png" );
				NextStageTime = 300f * StageTimeMultiplier;
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

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !NextStageTime ) return;

			if ( Stage == RoundStage.Start )
			{
				Announcements.Send( To.Everyone, "Gold II", "Gold generators now generate faster!", "textures/items/gold.png" );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.GoldII;
			}
			else if ( Stage == RoundStage.GoldII )
			{
				Announcements.Send( To.Everyone, "Crystal II", "Crystal generators now generate faster!", "textures/items/crystal.png" );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.CrystalII;
			}
			else if ( Stage == RoundStage.CrystalII )
			{
				Announcements.Send( To.Everyone, "Gold III", "Gold generators now even generate faster!", "textures/items/gold.png" );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.GoldIII;
			}
			else if ( Stage == RoundStage.GoldIII )
			{
				Announcements.Send( To.Everyone, "Crystal III", "Crystal generators now generate even faster!", "textures/items/crystal.png" );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.CrystalIII;
			}
			else if ( Stage == RoundStage.CrystalIII )
			{
				Announcements.Send( To.Everyone, "Warning", "All team Cores will self-destruct in 60 seconds!", "textures/ui/logo_spinner.png" );
				NextStageTime = 60f * StageTimeMultiplier;
				Stage = RoundStage.NoBeds;
			}
			else if ( Stage == RoundStage.NoBeds )
			{
				Announcements.Send( To.Everyone, "Sudden Death", "All team Cores have self-destructed!", "textures/ui/skull.png" );
				NextStageTime = 300f * StageTimeMultiplier;
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
