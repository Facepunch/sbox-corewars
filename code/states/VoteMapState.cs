using Sandbox;

namespace Facepunch.CoreWars
{
	public partial class VoteMapState : BaseState
	{
		private VoteMapEntity VoteMap { get; set; }

		public override void OnEnter()
		{
			if ( IsServer )
			{
				VoteMap = new();
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( VoteMap.IsValid() && VoteMap.VoteTimeLeft )
			{
				Global.ChangeLevel( VoteMap.WinningMap );
			}
		}
	}
}
