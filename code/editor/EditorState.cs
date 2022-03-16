using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Editor
{
	public class EditorState : BaseState
	{
		public virtual async Task LoadInitialChunks( VoxelWorld world, string fileName )
		{
			await world.LoadFromFile( "editor.voxels" );
		}

		public virtual void SaveChunksToDisk( VoxelWorld world )
		{
			world.SaveToFile( "editor.voxels" );
		}

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

		public override void OnPlayerJoined( Player player )
		{
			player.Respawn();
		}
	}
}
