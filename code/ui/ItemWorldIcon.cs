using Sandbox.UI.Construct;
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
			Icon = Container.Add.Image( entity.Item.Instance.Icon, "icon" );
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

			transform.Position = Entity.IconPosition;
			transform.Rotation = targetRotation;
			transform.Scale = 1.5f;

			Transform = transform;

			base.Tick();
		}
	}
}
