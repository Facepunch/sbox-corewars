﻿using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class SimpleRadialMenu : Panel
	{
		public virtual InputButton Button => InputButton.Score;

		public List<RadialMenuItem> Items { get; private set; }
		public RadialMenuItem Hovered { get; private set; }
		public Panel ItemContainer { get; set; }
		public string Title => Hovered?.Title ?? string.Empty;
		public string Description => Hovered?.Description ?? string.Empty;
		public Panel Dot { get; set; }

		private TimeSince LastCloseTime { get; set; }
		private Vector2 VirtualMouse { get; set; }
		private bool IsOpen { get; set; }

		public void Initialize()
		{
			Items ??= new();

			foreach ( var item in Items )
			{
				item.Delete();
			}

			Items.Clear();

			Populate();
		}

		public void AddTool( TypeDescription type )
		{
			var item = ItemContainer.AddChild<RadialMenuItem>();
			item.Title = type.Title;
			item.Description = type.Description;
			item.SetIcon( type.Icon );
			item.OnSelected = () => EditorPlayer.ChangeToolTo( type.Identity );
			Items.Add( item );
		}

		public void AddAction( string title, string description, string icon, Action callback )
		{
			var item = ItemContainer.AddChild<RadialMenuItem>();
			item.Title = title;
			item.Description = description;
			item.SetIcon( icon );
			item.OnSelected = callback;
			Items.Add( item );
		}

		[Event.BuildInput]
		public void BuildInput( InputBuilder builder )
		{
			var shouldOpen = ShouldOpen( builder );

			if ( builder.Pressed( Button ) && shouldOpen )
			{
				VirtualMouse = Screen.Size * 0.5f;
				IsOpen = true;
			}

			if ( builder.Released( Button ) || !shouldOpen )
			{
				IsOpen = false;
			}

			if ( IsOpen )
			{
				VirtualMouse += new Vector2( builder.AnalogLook.Direction.y, builder.AnalogLook.Direction.z ) * -500f;

				var lx = VirtualMouse.x - Box.Left;
				var ly = VirtualMouse.y - Box.Top;

				RadialMenuItem closestItem = null;
				var closestDistance = 0f;

				if ( VirtualMouse.Distance( Screen.Size * 0.5f ) >= Box.Rect.Size.x * 0.1f )
				{
					foreach ( var item in Items )
					{
						var distance = item.Box.Rect.Center.Distance( VirtualMouse );

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

				if ( Hovered != null && builder.Down( InputButton.PrimaryAttack ) )
				{
					Hovered.OnSelected?.Invoke();
					LastCloseTime = 0f;
					IsOpen = false;
				}

				builder.AnalogLook = Angles.Zero;
			}

			if ( IsOpen || LastCloseTime < 0.1f )
			{
				builder.ClearButton( InputButton.PrimaryAttack );
				builder.ClearButton( InputButton.SecondaryAttack );
			}
		}

		public virtual void Populate()
		{

		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			foreach ( var item in Items )
			{
				item.IsSelected = (Hovered == item);

				var fItemCount = (float)Items.Count;
				var maxItemScale = 1.2f;
				var minItemScale = 0.9f - fItemCount.Remap( 4f, 10f, 0f, 0.35f );
				var distanceToMouse = item.Box.Rect.Center.Distance( VirtualMouse );
				var distanceToScale = distanceToMouse.Remap( 0f, item.Box.Rect.Size.Length * 1.5f, maxItemScale, minItemScale ).Clamp( minItemScale, maxItemScale );

				var tx = new PanelTransform();
				tx.AddScale( distanceToScale );
				item.Style.Transform = tx;
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "hidden", IsHidden );
			Initialize();
			base.PostTemplateApplied();
		}

		protected virtual bool ShouldOpen( InputBuilder builder )
		{
			return true;
		}

		protected override void FinalLayoutChildren()
		{
			var radius = Box.Rect.Size.x * 0.5f;
			var center = Box.Rect.WithoutPosition.Center;

			for ( var i = 0; i < Items.Count; i++ )
			{
				var theta = (i * 2f * Math.PI / Items.Count) - Math.PI;
				var x = (float)Math.Sin( theta ) * radius;
				var y = (float)Math.Cos( theta ) * radius;
				var item = Items[i];

				item.Style.Left = Length.Pixels( (center.x + x) * ScaleFromScreen );
				item.Style.Top = Length.Pixels( (center.y + y) * ScaleFromScreen );
			}

			base.FinalLayoutChildren();
		}

		private bool IsHidden() => !IsOpen;
	}
}
