using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	public partial class BaseGenerator : ModelEntity, ISourceEntity
	{
		private TimeUntil NextGeneration { get; set; }

		public virtual void Serialize( BinaryWriter writer )
		{

		}

		public virtual void Deserialize( BinaryReader reader )
		{

		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !Game.IsState<GameState>() && !Game.IsState<LobbyState>() )
				return;

			if ( NextGeneration )
			{
				GenerateItems();
				NextGeneration = CalculateNextGenerationTime();
			}
		}

		protected virtual void GenerateItems()
		{

		}

		protected virtual float CalculateNextGenerationTime()
		{
			return 10f;
		}
	}
}
