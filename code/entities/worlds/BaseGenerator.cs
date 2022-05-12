using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class BaseGenerator : ModelEntity, ISourceEntity, IResettable
	{
		private TimeUntil NextGenerateTime { get; set; }

		public virtual void Reset()
		{
			NextGenerateTime = GetNextGenerateTime();
		}

		public virtual void Serialize( BinaryWriter writer )
		{

		}

		public virtual void Deserialize( BinaryReader reader )
		{

		}

		protected void Generate<T>( int stackSize ) where T : ResourceItem
		{
			var itemsInArea = FindInSphere( Position, CollisionBounds.Size.Length * 2f )
				.OfType<ItemEntity>()
				.Where( entity => entity.Item.Instance is T )
				.Count();

			if ( itemsInArea >= 16 ) return;

			var item = InventorySystem.CreateItem<T>();
			item.StackSize = (ushort)stackSize;

			var entity = new ItemEntity();
			entity.Position = WorldSpaceBounds.Center + Vector3.Up * 64f;
			entity.SetItem( item );
			entity.ApplyLocalImpulse( Vector3.Random * 100f );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !Game.IsState<GameState>() )
				return;

			if ( NextGenerateTime )
			{
				GenerateItems();
				NextGenerateTime = GetNextGenerateTime();
			}

			OnGeneratorTick();
		}

		protected virtual void OnGeneratorTick()
		{

		}

		protected virtual void GenerateItems()
		{

		}

		protected virtual float GetNextGenerateTime()
		{
			return 10f;
		}
	}
}
