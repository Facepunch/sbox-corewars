using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Crystal Generator", Group = "Generators", EditorModel = "models/gameplay/resource_pool/resource_pool_crystal.vmdl" )]
	public class CrystalGenerator : BaseGenerator, ISourceEntity
	{
		private Particles Effect { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/resource_pool/resource_pool_crystal.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			Effect = Particles.Create( "particles/gameplay/resource_pool/resource_pool_crystal.vpcf", this );
			Effect.SetEntity( 0, this );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }

		protected override void ServerTick()
		{
			base.ServerTick();
		}

		protected override void GenerateItems()
		{
			var itemsInArea = FindInSphere( Position, CollisionBounds.Size.Length * 2f )
				.OfType<ItemEntity>()
				.Where( entity => entity.Item.Instance is CrystalItem )
				.Count();

			if ( itemsInArea >= 16 ) return;

			var item = InventorySystem.CreateItem<CrystalItem>();
			item.StackSize = 1;

			var entity = new ItemEntity();
			entity.Position = WorldSpaceBounds.Center + Vector3.Up * 64f;
			entity.SetItem( item );
			entity.ApplyLocalImpulse( Vector3.Random * 100f );
		}

		protected override float CalculateNextGenerationTime()
		{
			return 30f;
		}
	}
}
