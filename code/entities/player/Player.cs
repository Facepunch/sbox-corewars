using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }
		[BindComponent] public ChunkViewer ChunkViewer { get; }

		public DamageInfo LastDamageTaken { get; private set; }

		public Player() : base()
		{

		}

		public Player( Client client ) : this()
		{

		}

		public void LoadChunk( Chunk chunk )
		{
			if ( ChunkViewer.LoadedChunks.Contains( chunk.Index ) )
				return;

			ChunkViewer.LoadedChunks.Add( chunk.Index );

			var offset = chunk.Offset;
			var types = chunk.BlockTypes;
			var index = chunk.Index;

			ReceiveChunk( To.Single( Client ), offset.x, offset.y, offset.z, index, types );
		}

		[ClientRpc]
		public void ReceiveChunk( int x, int y, int z, int index, byte[] data )
		{
			Map.Current.ReceiveChunk( index, data );

			Log.Info( $"(#{NetworkIdent}) Received all bytes for chunk{x},{y},{z} ({data.Length / 1024}kb)" );
		}

		public void SetTeam( Team team )
		{
			Host.AssertServer();

			Team = team;
			OnTeamChanged( team );
		}

		public override void Spawn()
		{
			Components.Create<ChunkViewer>();

			EnableHideInFirstPerson = true;
			EnableAllCollisions = true;
			EnableDrawing = true;

			Camera = new FirstPersonCamera();

			Controller = new MoveController()
			{
				WalkSpeed = 195f,
				SprintSpeed = 375f
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
					Game.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, (byte)Rand.Int( 1, 3 ) );
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
