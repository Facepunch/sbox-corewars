using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Utility;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class BaseGenerator : ModelEntity, ISourceEntity, IResettable, IHudRenderer
	{
		protected virtual string HudIconPath => null;
		protected virtual bool ShowHudIcon => false;

		[Net] public RealTimeUntil NextGenerateTime { get; protected set; }
		[Net] public float NextGenerateDuration { get; protected set; }

		public virtual void Reset()
		{
			NextGenerateDuration = GetNextGenerateTime();
			NextGenerateTime = NextGenerateDuration;
		}

		public virtual void Serialize( BinaryWriter writer )
		{

		}

		public virtual void Deserialize( BinaryReader reader )
		{

		}

		public virtual void RenderHud( Vector2 screenSize )
		{
			if ( !ShowHudIcon || string.IsNullOrEmpty( HudIconPath ) )
				return;

			var draw = Util.Draw.Reset();
			var position = (WorldSpaceBounds.Center + Vector3.Up * 96f).ToScreen();
			var iconSize = 32f;
			var iconAlpha = 1f;

			position.x *= screenSize.x;
			position.y *= screenSize.y;
			position.x -= iconSize * 0.5f;
			position.y -= iconSize * 0.5f;

			var distanceToPawn = Local.Pawn.Position.Distance( Position );

			if ( distanceToPawn <= 1024f )
				iconAlpha = distanceToPawn.Remap( 512f, 1024f, 0f, 1f );
			else if ( distanceToPawn > 2048f )
				iconAlpha = distanceToPawn.Remap( 2048f, 3072f, 1f, 0f );

			draw.Color = Color.White.WithAlpha( iconAlpha );
			draw.BlendMode = BlendMode.Normal;
			draw.Image( HudIconPath, new Rect( position.x, position.y, iconSize, iconSize ) );

			var outerBox = new Rect( position.x, position.y + iconSize + 8f, iconSize, 6f );
			var innerBox = outerBox.Shrink( 2f, 2f, 2f, 2f );
			var fraction = (1f / NextGenerateDuration) * NextGenerateTime;

			innerBox.Width *= fraction;

			var innerColor = Color.Lerp( Color.Green, Color.Red, fraction );

			draw.Color = Color.Black.WithAlpha( iconAlpha );
			draw.Box( outerBox, new Vector4( 2f, 2f, 2f, 2f ) );

			draw.Color = innerColor.WithAlpha( iconAlpha * 0.7f );
			draw.Box( innerBox );
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

			PlaySound( "item.dropped" );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( !Game.IsState<GameState>() )
				return;

			if ( NextGenerateTime )
			{
				GenerateItems();
				NextGenerateDuration = GetNextGenerateTime();
				NextGenerateTime = NextGenerateDuration;
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
