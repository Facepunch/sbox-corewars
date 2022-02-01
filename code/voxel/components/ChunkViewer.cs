using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public partial class ChunkViewer : EntityComponent
	{
		[Net] public IList<Chunk> LoadedChunks { get; private set; }

		protected override void OnActivate()
		{
			LoadedChunks = new List<Chunk>();

			base.OnActivate();
		}

		// TODO: Our RPCs for receiving chunks will go here, but entity components cannot have RPCs yet.
	}
}
