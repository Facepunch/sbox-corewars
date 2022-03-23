using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class ActiveDraggable : Panel
	{
		public static ActiveDraggable Current { get; private set; }

		public static void Start( IDraggable draggable )
		{
			var active = Game.Hud.AddChild<ActiveDraggable>();
			active.SetDraggable( draggable );
			active.UpdatePosition();
		}

		public static void Stop( IDraggable draggable )
		{
			if ( Current?.Draggable == draggable )
			{
				Current.UpdateDroppable();

				if ( Current.Droppable?.CanDrop( draggable ) ?? false )
				{
					Current.Droppable.OnDrop( Current.Draggable );
				}

				Current.Delete();
				Current = null;
			}
		}

		public IDraggable Draggable { get; private set; }

		private IDroppable Droppable { get; set; }

		public ActiveDraggable()
		{
			Current?.Delete( true );
			Current = this;
		}

		public void UpdatePosition()
		{
			Style.Left = Length.Pixels( Mouse.Position.x * ScaleFromScreen );
			Style.Top = Length.Pixels( Mouse.Position.y * ScaleFromScreen );
		}

		public void SetDraggable( IDraggable draggable )
		{
			Draggable = draggable;
			Style.SetBackgroundImage( draggable.GetIconTexture() );
			Style.Width = Length.Pixels( draggable.IconSize * ScaleFromScreen );
			Style.Height = Length.Pixels( draggable.IconSize * ScaleFromScreen );
		}

		public override void Tick()
		{
			UpdatePosition();
			UpdateDroppable();

			base.Tick();
		}

		public override void OnHotloaded()
		{
			Delete();
			Current = null;

			base.OnHotloaded();
		}

		public override void OnDeleted()
		{
			Droppable?.RemoveClass( "valid-drag" );
			Droppable?.RemoveClass( "invalid-drag" );

			base.OnDeleted();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}

		public void UpdateDroppable()
		{
			var droppable = FindDroppable();

			if ( Droppable != null && droppable != Droppable )
			{
				Droppable.RemoveClass( "valid-drag" );
				Droppable.RemoveClass( "invalid-drag" );
			}

			Droppable = droppable;

			if ( Droppable != null )
			{
				if ( Droppable.CanDrop( Draggable ) )
					Droppable.AddClass( "valid-drag" );
				else
					Droppable.AddClass( "invalid-drag" );
			}
		}

		private IDroppable FindDroppable( Panel root = null )
		{
			root ??= Game.Hud;

			if ( root is IDroppable droppable )
			{
				return droppable;
			}

			foreach ( var child in root.Children )
			{
				if ( !child.Box.Rect.IsInside( Mouse.Position ) )
					continue;

				var panel = FindDroppable( child );

				if ( panel != null )
					return panel;
			}

			return null;
		}
	}
}
