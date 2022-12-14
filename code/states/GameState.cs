﻿using Sandbox;
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
			if ( Game.IsServer )
			{
				IResettable.ResetAll();

				foreach ( var player in Entity.All.OfType<CoreWarsPlayer>() )
				{
					player.AssignRandomTeam( true );
					player.RespawnWhenAvailable();

					var playerId = player.Client.SteamId;
					PlayerToTeam[playerId] = player.Team;
				}

				UI.Announcements.Send( To.Everyone, "Core Wars", "Destroy your opponents Core to prevent them respawning!", RoundStage.Start.GetIcon() );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.Start;
			}
		}

		public override bool CanHearPlayerVoice( IClient a, IClient b )
		{
			if ( a.Pawn is not CoreWarsPlayer src )
				return false;

			if ( b.Pawn is not CoreWarsPlayer dest )
				return false;

			return src.Team == dest.Team;
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( CoreWarsPlayer player )
		{
			var playerId = player.Client.SteamId;

			if ( !PlayerToTeam.ContainsKey( playerId ) )
				player.AssignRandomTeam( true );
			else
				player.SetTeam( PlayerToTeam[playerId] );

			PlayerToTeam[playerId] = player.Team;

			player.RespawnWhenAvailable();
		}

		protected virtual void OnStageChanged( RoundStage stage )
		{

		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !NextStageTime ) return;

			if ( Stage == RoundStage.Start )
			{
				UI.Announcements.Send( To.Everyone, "Gold II", "Gold generators now generate faster!", RoundStage.GoldII.GetIcon() );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.GoldII;
			}
			else if ( Stage == RoundStage.GoldII )
			{
				UI.Announcements.Send( To.Everyone, "Crystal II", "Crystal generators now generate faster!", RoundStage.CrystalII.GetIcon() );
				Airdrop.Create();
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.CrystalII;
			}
			else if ( Stage == RoundStage.CrystalII )
			{
				UI.Announcements.Send( To.Everyone, "Gold III", "Gold generators now even generate faster!", RoundStage.GoldIII.GetIcon() );
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.GoldIII;
			}
			else if ( Stage == RoundStage.GoldIII )
			{
				UI.Announcements.Send( To.Everyone, "Crystal III", "Crystal generators now generate even faster!", RoundStage.CrystalIII.GetIcon() );
				Airdrop.Create();
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.CrystalIII;
			}
			else if ( Stage == RoundStage.CrystalIII )
			{
				UI.Announcements.Send( To.Everyone, "Warning", "All team Cores will self-destruct in 60 seconds!", RoundStage.NoCores.GetIcon() );
				NextStageTime = 60f * StageTimeMultiplier;
				Stage = RoundStage.NoCores;
			}
			else if ( Stage == RoundStage.NoCores )
			{
				UI.Announcements.Send( To.Everyone, "Sudden Death", "All team Cores have self-destructed!", RoundStage.SuddenDeath.GetIcon() );
				Airdrop.Create();
				NextStageTime = 300f * StageTimeMultiplier;
				Stage = RoundStage.SuddenDeath;

				var cores = Entity.All.OfType<TeamCore>();

				foreach ( var core in cores )
				{
					core.Explode();
				}
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
