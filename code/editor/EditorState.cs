using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorState : BaseState
	{
		[Net] public string CurrentFileName { get; set; }

		private RealTimeUntil NextBackupSave { get; set; }

		public override void OnEnter()
		{
			if ( Host.IsServer )
			{
				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.Respawn();
				}
			}
		}

		public override void OnLeave()
		{

		}

		public override bool CanHearPlayerVoice( Client a, Client b )
		{
			return true;
		}

		public override void OnPlayerJoined( Player player )
		{
			player.Respawn();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( NextBackupSave && !string.IsNullOrEmpty( CurrentFileName ) )
			{
				Game.SaveEditorMap( CurrentFileName, true );
				NextBackupSave = 60f;
			}
		}
	}
}
