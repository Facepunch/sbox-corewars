using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Draggable : Panel
	{
		public static Draggable Current { get; private set; }

		public static void Start( IDraggable draggable )
		{
			var active = Game.Hud.AddChild<Draggable>();
			active.SetDraggable( draggable );
			active.UpdatePosition();
		}

		public static void Stop( IDraggable draggable )
		{
			if ( Current?.ActiveDraggable == draggable )
			{
				Current.UpdateDroppable();

				if ( Current.ActiveDroppable?.CanDrop( draggable ) ?? false )
				{
					Current.ActiveDroppable.OnDrop( Current.ActiveDraggable );
				}

				Current.Delete();
				Current = null;
			}
		}

		public IDraggable ActiveDraggable { get; private set; }

		private IDroppable ActiveDroppable { get; set; }

		public Draggable()
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
			ActiveDraggable = draggable;
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
			ActiveDroppable?.RemoveClass( "valid-drag" );
			ActiveDroppable?.RemoveClass( "invalid-drag" );

			base.OnDeleted();
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}

		public void UpdateDroppable()
		{
			var droppable = FindDroppable();

			if ( ActiveDroppable != null && droppable != ActiveDroppable )
			{
				ActiveDroppable.RemoveClass( "valid-drag" );
				ActiveDroppable.RemoveClass( "invalid-drag" );
			}

			ActiveDroppable = droppable;

			if ( ActiveDroppable != null )
			{
				if ( ActiveDroppable.CanDrop( ActiveDraggable ) )
					ActiveDroppable.AddClass( "valid-drag" );
				else
					ActiveDroppable.AddClass( "invalid-drag" );
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
