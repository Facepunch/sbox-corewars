using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Generator", Group = "Generators", EditorModel = "models/gameplay/resource_pool/resource_pool.vmdl" )]
	public partial class TeamGenerator : BaseGenerator
	{
		[EditorProperty, Net] public Team Team { get; set; }

		[Net] public int UpgradeLevel { get; private set; }

		private Particles Effect { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Effect = Particles.Create( "particles/gameplay/resource_pool/resource_pool.vpcf", this );
			Effect.SetEntity( 0, this );

			UpgradeLevel = 0;

			base.Spawn();
		}

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public override void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			RenderColor = Team.GetColor();
		}

		protected override void ServerTick()
		{
			base.ServerTick();
		}

		protected override void GenerateItems()
		{
			var itemsInArea = FindInSphere( Position, CollisionBounds.Size.Length * 2f )
				.OfType<ItemEntity>()
				.Where( entity => entity.Item.Instance is IronItem )
				.Count();

			if ( itemsInArea >= 16 ) return;

			var item = InventorySystem.CreateItem<IronItem>();
			item.StackSize = 4;

			var entity = new ItemEntity();
			entity.Position = WorldSpaceBounds.Center + Vector3.Up * 64f;
			entity.SetItem( item );
			entity.ApplyLocalImpulse( Vector3.Random * 100f );
		}

		protected override float CalculateNextGenerationTime()
		{
			if ( UpgradeLevel == 0 ) return 10f;
			if ( UpgradeLevel == 1 ) return 8f;
			if ( UpgradeLevel == 2 ) return 6f;
			return 4f;
		}
	}
}
