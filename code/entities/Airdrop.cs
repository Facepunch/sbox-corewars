using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Airdrop : ModelEntity
	{
		[ConCmd.Server( "cw_create_airdrop" )]
		public static void CreateAirdropCmd()
		{
			Create();
		}

		public static bool Create()
		{
			var spawnpoints = All.OfType<AirdropSpawnpoint>().ToList();

			if ( spawnpoints.Count > 0 )
			{
				var world = VoxelWorld.Current;
				var spawnpoint = Rand.FromList( spawnpoints );

				var airdrop = new Airdrop();
				airdrop.Transform = spawnpoint.Transform;
				airdrop.Position = airdrop.Position.WithZ( world.MaxSize.z * world.VoxelSize * 2f );

				Hud.ToastAll( "An airdrop shop is incoming!", "textures/ui/airdrop.png" );

				return true;
			}

			return false;
		}

		public bool HasLanded { get; private set; }

		public override void Spawn()
		{
			SetModel( "models/rust_props/wooden_crates/wooden_crate_d.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;

			base.Spawn();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( HasLanded ) return;

			var velocity = Vector3.Down * 300f * Time.Delta;
			var position = Position + velocity;
			var trace = Trace.Sweep( PhysicsBody, Transform, Transform.WithPosition( position ) )
				.Ignore( this )
				.Run();

			Position = trace.EndPosition;
			HasLanded = trace.Hit;
		}
	}
}
