using Sandbox;

namespace Facepunch.CoreWars
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }

		public DamageInfo LastDamageTaken { get; private set; }

		private Clothing.Container Clothing { get; set; }

		public Player() : base()
		{

		}

		public Player( Client client ) : this()
		{
			Clothing = new();
			Clothing.LoadFromClient( client );
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
			EnableAllCollisions = false;
			SetModel( "models/citizen/citizen.vmdl" );

			base.Spawn();
		}

		public override void Respawn()
		{
			Game.Current?.PlayerRespawned( this );
			Clothing.DressEntity( this );

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
			base.Simulate( client );
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
