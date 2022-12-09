using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class BasePlayer : AnimatedEntity
	{
		[Net, Predicted] public BaseMoveController Controller { get; set; }
		[Net, Predicted] public Entity ActiveChild { get; set; }
		[ClientInput] public Vector3 InputDirection { get; protected set; }
		[ClientInput] public Entity ActiveChildInput { get; set; }
		[ClientInput] public Angles ViewAngles { get; set; }

		public Angles OriginalViewAngles { get; protected set; }

		protected TimeSince TimeSinceLastFootstep { get; set; }
		protected Entity LastActiveChild { get; set; }

		public Vector3 EyePosition
		{
			get => Transform.PointToWorld( EyeLocalPosition );
			set => EyeLocalPosition = Transform.PointToLocal( value );
		}

		[Net, Predicted]
		public Vector3 EyeLocalPosition { get; set; }

		public Rotation EyeRotation
		{
			get => Transform.RotationToWorld( EyeLocalRotation );
			set => EyeLocalRotation = Transform.RotationToLocal( value );
		}

		[Net, Predicted]
		public Rotation EyeLocalRotation { get; set; }

		public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

		public virtual void Respawn()
		{
			LifeState = LifeState.Alive;
			Health = 100f;
			Velocity = Vector3.Zero;

			CreateHull();
			ResetInterpolation();
		}

		public override void BuildInput()
		{
			OriginalViewAngles = ViewAngles;
			InputDirection = Input.AnalogMove;

			if ( Input.StopProcessing )
				return;

			var look = Input.AnalogLook;

			if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			{
				look = look.WithYaw( look.yaw * -1f );
			}

			var viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
			viewAngles.roll = 0f;
			ViewAngles = viewAngles.Normal;

			ActiveChild?.BuildInput();
		}

		protected virtual void CreateHull()
		{
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16f, -16f, 0f ), new Vector3( 16f, 16f, 72f ) );
			EnableHitboxes = true;
		}

		protected virtual void SimulateActiveChild( Entity child )
		{
			if ( Prediction.FirstTime )
			{
				if ( LastActiveChild != child )
				{
					OnActiveChildChanged( LastActiveChild, child );
					LastActiveChild = child;
				}
			}

			if ( !LastActiveChild.IsValid() )
				return;

			if ( LastActiveChild.IsAuthority )
			{
				LastActiveChild.Simulate( Client );
			}
		}

		protected virtual void OnActiveChildChanged( Entity previous, Entity next )
		{
			if ( previous is Weapon previousWeapon )
			{
				previousWeapon?.ActiveEnd( this, previousWeapon.Owner != this );
			}

			if ( next is Weapon nextWeapon )
			{
				nextWeapon?.ActiveStart( this );
			}
		}

		protected virtual float GetFootstepVolume()
		{
			return Velocity.WithZ( 0f ).Length.LerpInverse( 0f, 300f ) * 1f;
		}
	}
}
