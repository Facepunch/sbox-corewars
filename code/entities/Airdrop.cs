using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Airdrop : ModelEntity, IUsable, IItemStore
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

		public List<BaseShopItem> Items { get; private set; } = new();
		public float MaxUseDistance => 300f;
		public bool HasLanded { get; private set; }

		public bool IsUsable( Player player )
		{
			return true;
		}

		public void OnUsed( Player player )
		{
			OpenForClient( To.Single( player ) );
		}

		public override void Spawn()
		{
			SetModel( "models/rust_props/wooden_crates/wooden_crate_d.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;
			AddAllItems();

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			AddAllItems();
			base.ClientSpawn();
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

		private void AddAllItems()
		{
			var types = TypeLibrary.GetTypes<BaseShopItem>();

			foreach ( var type in types )
			{
				if ( type.IsAbstract || type.IsGenericType ) continue;
				var description = TypeLibrary.GetDescription( type );
				if ( !description.HasTag( "airdrop" ) ) continue;
				var item = TypeLibrary.Create<BaseShopItem>( type );
				Items.Add( item );
			}
		}

		[ClientRpc]
		private void OpenForClient()
		{
			AirdropStore.Current.SetAirdrop( this );
			AirdropStore.Current.Open();
		}
	}
}
