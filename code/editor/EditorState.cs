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

		public ActionHistory<EditorAction> UndoStack { get; private set; }
		public ActionHistory<EditorAction> RedoStack { get; private set; }

		public override void OnEnter()
		{
			if ( Host.IsServer )
			{
				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.Respawn();
				}

				UndoStack = new( 20 );
				RedoStack = new( 20 );
			}
		}

		public void Perform( EditorAction action )
		{
			Host.AssertServer();
			UndoStack.Push( action );
			action.Perform();
		}

		public void Undo()
		{
			Host.AssertServer();

			if ( UndoStack.TryPop( out var action ) )
			{
				RedoStack.Push( action );
				action.Undo();
			}
		}

		public void Redo()
		{
			Host.AssertServer();

			if ( RedoStack.TryPop( out var action ) )
			{
				UndoStack.Push( action );
				action.Perform();
			}
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			player.Respawn();
		}
	}
}
