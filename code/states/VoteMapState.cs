using Sandbox;
using Facepunch.CoreWars.UI;

namespace Facepunch.CoreWars
{
	public partial class VoteMapState : BaseState
	{
		private VoteMapEntity VoteMap { get; set; }

		public override void OnEnter()
		{
			if ( Game.IsServer )
			{
				VoteMap = new();
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( VoteMap.IsValid() && VoteMap.VoteTimeLeft )
			{
				Game.ChangeLevel( VoteMap.WinningMap );
			}
		}
	}
}
