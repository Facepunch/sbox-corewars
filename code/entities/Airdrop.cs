using System.Collections.Generic;
using Facepunch.Voxels;
using Facepunch.CoreWars.Utility;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Airdrop : ModelEntity, IUsable, IItemStore, IHudRenderer
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

				UI.Hud.ToastAll( "An airdrop shop is incoming!", "textures/ui/airdrop.png" );
				return true;
			}

			return false;
		}

		[Net] public RealTimeUntil TimeUntilDestroy { get; private set; }
		[Net] public bool HasLanded { get; private set; }

		public float TimeToLiveFor { get; private set; } = 180f;
		public List<BaseShopItem> Items { get; private set; } = new();
		public float MaxUseDistance => 300f;

		private bool HasPlayedLandSound { get; set; }
		private Sound FallingSound { get; set; }
		private Particles Effect { get; set; }

		public bool IsUsable( Player player )
		{
			return true;
		}

		public void OnUsed( Player player )
		{
			OpenForClient( To.Single( player ) );
		}

		public virtual void RenderHud( Vector2 screenSize )
		{
			if ( !HasLanded ) return;

			var draw = Util.Draw.Reset();
			var position = (WorldSpaceBounds.Center + Vector3.Up * 96f).ToScreen();
			var iconSize = 64f;
			var iconAlpha = 1f;

			position.x *= screenSize.x;
			position.y *= screenSize.y;
			position.x -= iconSize * 0.5f;
			position.y -= iconSize * 0.5f;

			var distanceToPawn = Local.Pawn.Position.Distance( Position );

			if ( distanceToPawn <= 1024f )
				iconAlpha = distanceToPawn.Remap( 512f, 1024f, 0f, 1f );
			else if ( distanceToPawn > 3072f )
				iconAlpha = distanceToPawn.Remap( 3072f, 4096f, 1f, 0f );

			draw.Color = Color.White.WithAlpha( iconAlpha );
			draw.BlendMode = BlendMode.Normal;
			draw.Image( "textures/ui/airdrop.png", new Rect( position.x, position.y, iconSize, iconSize ) );

			var outerBox = new Rect( position.x, position.y + iconSize + 8f, iconSize, 6f );
			var innerBox = outerBox.Shrink( 2f, 2f, 2f, 2f );
			var fraction = (1f / TimeToLiveFor) * TimeUntilDestroy;

			innerBox.Width *= fraction;

			var innerColor = Color.Lerp( Color.Red, Color.Green, fraction );

			draw.Color = Color.Black.WithAlpha( iconAlpha );
			draw.Box( outerBox, new Vector4( 4f, 4f, 4f, 4f ) );

			draw.Color = innerColor.WithAlpha( iconAlpha * 0.7f );
			draw.Box( innerBox );
		}

		public override void Spawn()
		{
			SetModel( "models/gameplay/temp/shrine_test/air_drop_shrine.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;
			AddAllItems();

			Effect = Particles.Create( "particles/gameplay/air_drop/air_drop.vpcf", this );
			Effect.SetEntity( 0, this );

			FallingSound = PlaySound( "airdrop.falling" );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			AddAllItems();
			base.ClientSpawn();
		}

		public override void OnNewModel( Model model )
		{
			if ( IsClient )
			{
				VoxelWorld.RegisterVoxelModel( this );
			}

			base.OnNewModel( model );
		}

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				VoxelWorld.UnregisterVoxelModel( this );
			}

			Effect?.Destroy();

			base.OnDestroy();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			var velocity = Vector3.Down * 300f * Time.Delta;
			var trace = Trace.Sweep( PhysicsBody, Transform, Transform.WithPosition( Position + velocity ) )
				.Ignore( this )
				.Run();

			Position = trace.EndPosition;

			if ( HasLanded )
			{
				if ( TimeUntilDestroy ) Delete();
				return;
			}

			HasLanded = trace.Hit;

			if ( !HasLanded && !HasPlayedLandSound )
			{
				var ticksPerSecond = (1f / Time.Delta);

				// Let's check if we're going to land shortly.
				trace = Trace.Sweep( PhysicsBody, Transform, Transform.WithPosition( Position + velocity * (ticksPerSecond * 0.85f) ) )
					.Ignore( this )
					.Run();

				if ( trace.Hit )
				{
					HasPlayedLandSound = true;
					PlaySound( "airdrop.land" );
				}
			}

			if ( HasLanded )
			{
				var landFx = Particles.Create( "particles/gameplay/air_drop/air_drop_land/air_drop_land.vpcf", this );
				landFx.SetEntity( 0, this );

				TimeUntilDestroy = TimeToLiveFor;
				FallingSound.Stop();
				Effect?.Destroy();
			}
		}

		private void AddAllItems()
		{
			var types = TypeLibrary.GetTypes<BaseShopItem>();

			foreach ( var type in types )
			{
				if ( type.IsAbstract || type.IsGenericType ) continue;
				if ( !type.HasTag( "airdrop" ) ) continue;
				var item = type.Create<BaseShopItem>();
				Items.Add( item );
			}
		}

		[ClientRpc]
		private void OpenForClient()
		{
			UI.AirdropStore.Current.SetAirdrop( this );
			UI.AirdropStore.Current.Open();
		}
	}
}
