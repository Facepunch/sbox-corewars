using Sandbox;

namespace Facepunch.CoreWars
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }

		public DamageInfo LastDamageTaken { get; private set; }

		public Player() : base()
		{

		}

		public Player( Client client ) : this()
		{

		}

		public void SetTeam( Team team )
		{
			Host.AssertServer();

			Team = team;
			OnTeamChanged( team );
		}

		public override void Spawn()
		{
			EnableHideInFirstPerson = true;
			EnableAllCollisions = true;
			EnableDrawing = true;

			Camera = new FirstPersonCamera();

			Controller = new MoveController()
			{
				WalkSpeed = 300f,
				SprintSpeed = 500f
			};

			Animator = new PlayerAnimator();

			SetModel( "models/citizen/citizen.vmdl" );

			base.Spawn();
		}

		public override void Respawn()
		{
			Game.Current?.PlayerRespawned( this );

			base.Respawn();
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );
		}

		public override void FrameSimulate( Client client )
		{
			base.FrameSimulate( client );
		}

		public override void Simulate( Client client )
		{
			if ( IsServer )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					Game.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, (byte)Rand.Int( 1, 5 ) );
				}
				else if ( Input.Pressed( InputButton.Attack2 ) )
				{
					Game.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 );
				}
			}

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );
		}

		public override void TakeDamage( DamageInfo info )
		{
			LastDamageTaken = info;

			base.TakeDamage( info );
		}

		protected virtual void OnTeamChanged( Team team )
		{

		}
	}
}
