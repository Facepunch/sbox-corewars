using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	public interface IKillFeedInfo
	{
		public string[] KillFeedReasons { get; }
		public string KillFeedIcon { get; }
		public string KillFeedName { get; }
		public Team KillFeedTeam { get; }
	}
}
