using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorToolSelector : Panel
	{
		public static EditorToolSelector Current { get; private set; }

		public List<EditorToolItem> Items { get; private set; }
		public EditorToolItem Hovered { get; private set; }
		public string Title => Hovered?.Attribute?.Title ?? string.Empty;
		public string Description => Hovered?.Attribute?.Description ?? string.Empty;
		public Panel Dot { get; set; }

		private TimeSince LastCloseTime { get; set; }
		private Vector2 VirtualMouse { get; set; }
		private bool IsOpen { get; set; }

		public EditorToolSelector()
		{
			Current = this;
		}

		public void Initialize()
		{
			Items ??= new();

			foreach ( var item in Items )
			{
				item.Delete();
			}

			Items.Clear();

			var available = Library.GetAttributes<EditorToolLibraryAttribute>();

			foreach ( var attribute in available )
			{
				var item = AddChild<EditorToolItem>();
				item.SetLibraryItem( attribute );
				Items.Add( item );
			}
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			foreach ( var item in Items )
			{
				item.IsSelected = (Hovered == item);
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "hidden", IsHidden );
			Initialize();
			base.PostTemplateApplied();
		}

		private bool IsHidden() => !IsOpen;

		public override void OnLayout( ref Rect layoutRect )
		{
			base.OnLayout( ref layoutRect );

			var radius = layoutRect.Size.x * 0.5f;
			var center = layoutRect.WithoutPosition.Center;

			for ( var i = 0; i < Items.Count; i++ )
			{
				var theta = (i * 2f * Math.PI / Items.Count) - Math.PI;
				var x = (float)Math.Sin( theta ) * radius;
				var y = (float)Math.Cos( theta ) * radius;
				var item = Items[i];

				item.Style.Left = Length.Pixels( (center.x + x) * ScaleFromScreen );
				item.Style.Top = Length.Pixels( (center.y + y) * ScaleFromScreen  );
			}
		}

		[Event.BuildInput]
		private void BuildInput( InputBuilder builder )
		{
			if ( builder.Pressed( InputButton.Score ) )
			{
				VirtualMouse = Screen.Size * 0.5f;
				IsOpen = true;
			}

			if ( builder.Released( InputButton.Score ) )
			{
				IsOpen = false;
			}

			if ( IsOpen )
			{
				VirtualMouse += new Vector2( builder.AnalogLook.Direction.y, builder.AnalogLook.Direction.z ) * -500f;

				var lx = VirtualMouse.x - Box.Left;
				var ly = VirtualMouse.y - Box.Top;

				EditorToolItem closestItem = null;
				var closestDistance = 0f;

				if ( VirtualMouse.Distance( Screen.Size * 0.5f ) >= 100f )
				{
					foreach ( var item in Items )
					{
						var distance = item.Box.Rect.Position.Distance( VirtualMouse );

						if ( closestItem == null || distance < closestDistance )
						{
							closestDistance = distance;
							closestItem = item;
						}
					}
				}

				Hovered = closestItem;

				Dot.Style.Left = Length.Pixels( lx * ScaleFromScreen );
				Dot.Style.Top = Length.Pixels( ly * ScaleFromScreen );

				if ( Hovered != null && builder.Down( InputButton.Attack1 ) )
				{
					EditorPlayer.ChangeToolTo( Hovered.Attribute.Identifier );
					LastCloseTime = 0f;
					IsOpen = false;
				}

				builder.AnalogLook = Angles.Zero;
			}

			if ( IsOpen || LastCloseTime < 0.1f )
			{
				builder.ClearButton( InputButton.Attack1 );
				builder.ClearButton( InputButton.Attack2 );
			}
		}
	}
}
