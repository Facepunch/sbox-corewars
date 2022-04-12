﻿using Sandbox.UI.Construct;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public class ItemWorldIcon : WorldPanel
	{
		public Panel Container { get; private set; }
		public Image Icon { get; private set; }
		public ItemEntity Entity { get; private set; }

		public ItemWorldIcon( ItemEntity entity )
		{
			StyleSheet.Load( "/ui/ItemWorldIcon.scss" );
			Container = Add.Panel( "container" );
			Entity = entity;
			Icon = Container.Add.Image( entity.Item.Instance.GetIcon(), "icon" );
		}

		public override void Tick()
		{
			if ( IsDeleting ) return;

			if ( !Entity.IsValid() )
			{
				Delete();
				return;
			}

			var transform = Transform;
			var targetRotation = Rotation.LookAt( CurrentView.Position - Position );

			transform.Position = Entity.WorldSpaceBounds.Center + Vector3.Up * (12f + MathF.Sin( Time.Now ) * 8f);
			transform.Rotation = targetRotation;
			transform.Scale = 2f;

			Transform = transform;

			base.Tick();
		}
	}
}