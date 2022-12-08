using Sandbox.UI.Construct;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Facepunch.CoreWars.UI
{
	public class ItemWorldIcon : WorldPanel
	{
		public Panel Container { get; private set; }
		public Panel Glow { get; private set; }
		public Image Icon { get; private set; }
		public ItemEntity Entity { get; private set; }

		public ItemWorldIcon( ItemEntity entity )
		{
			StyleSheet.Load( "/ui/ItemWorldIcon.scss" );
			Container = Add.Panel( "container" );
			Glow = Container.Add.Panel( "glow" );
			Entity = entity;
			Icon = Container.Add.Image( entity.Item.Icon, "icon" );

			var item = entity.Item;

			if ( item.IsValid() )
			{
				var shadowList = new ShadowList();
				var shadow = new Shadow()
				{
					OffsetX = 0f,
					OffsetY = 0f,
					Blur = 64f,
					Spread = 0f,
					Color = item.Color.Saturate( 0.5f ).WithAlpha( 0.5f )
				};
				shadowList.Add( shadow );

				Glow.Style.BoxShadow = shadowList;
			}
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
			var direction = (Camera.Position - Entity.IconPosition).Normal;
			var targetRotation = Rotation.LookAt( direction );

			transform.Position = Entity.IconPosition;
			transform.Rotation = targetRotation;
			transform.Scale = 1.5f;

			Transform = transform;

			base.Tick();
		}
	}
}
