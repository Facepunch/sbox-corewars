using Sandbox.UI.Construct;
using Sandbox.UI;
using System;
using Sandbox;

namespace Facepunch.CoreWars
{
	public class Nameplate : WorldPanel
	{
		public Panel Container { get; private set; }
		public Panel Dot { get; private set; }
		public Label NameLabel { get; private set; }
		public float StartFadeDistance { get; set; } = 375f;
		public float EndFadeDistance { get; set; } = 500f;
		public float StartDotDistance { get; set; } = 525f;
		public float EndDotDistance { get; set; } = 1250f;
		public INameplate Entity { get; private set; }

		public Nameplate( INameplate entity )
		{
			StyleSheet.Load( "/ui/Nameplate.scss" );
			Container = Add.Panel( "container" );
			Dot = Add.Panel( "dot" );
			NameLabel = Container.Add.Label( "", "name" );
			PanelBounds = new Rect( -1000, -1000, 2000, 2000 );
			Entity = entity;
		}

		public override void Tick()
		{
			if ( Local.Pawn is not Player player )
				return;

			if ( IsDeleting ) return;

			if ( !Entity.IsValid() )
			{
				Delete();
				return;
			}

			if ( !IsEntityVisible() )
			{
				SetClass( "hidden", true );
				return;
			}

			var transform = Transform;

			transform.Position = Entity.WorldSpaceBounds.Center + Vector3.Up * 56f;

			var targetRotation = Rotation.LookAt( CurrentView.Position - Position );
			transform.Rotation = targetRotation;

			var distanceToCamera = player.Position.Distance( Entity.Position );
			transform.Scale = distanceToCamera.Remap( 0f, EndFadeDistance, 1f, 3f );

			float opacity;
			if ( distanceToCamera >= StartFadeDistance )
			{
				var overlap = (distanceToCamera - StartFadeDistance);
				opacity = overlap.Remap( 0f, (EndFadeDistance - StartFadeDistance), 0f, 1f );
				Container.Style.Opacity = Math.Clamp( 1f - opacity, 0f, 1f );
			}
			else
			{
				Container.Style.Opacity = 1f;
			}

			var fadeStart = StartFadeDistance + (EndFadeDistance - StartFadeDistance) * 0.9f;
			opacity = distanceToCamera.Remap( fadeStart, EndFadeDistance, 0f, 1f );
			var fadeOutStart = EndDotDistance * 1.5f;

			if ( distanceToCamera >= fadeOutStart )
			{
				opacity = 1f - distanceToCamera.Remap( fadeOutStart, fadeOutStart + 1000f, 0f, 1f );
			}

			Dot.Style.Opacity = Math.Clamp( opacity, 0f, 1f );
			NameLabel.Text = Entity.DisplayName;

			Transform = transform;

			SetClass( "friendly", Entity.IsFriendly );
			SetClass( "hidden", Dot.Style.Opacity == 0f && Container.Style.Opacity == 0f );

			base.Tick();
		}

		private bool IsEntityVisible()
		{
			if ( Entity.LifeState == LifeState.Dead )
				return false;

			if ( Entity == Local.Pawn )
				return false;

			return true;
		}
	}
}
