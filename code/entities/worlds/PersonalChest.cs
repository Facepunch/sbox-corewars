using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Personal Chest", Group = "Gameplay", EditorModel = "models/editor/playerstart.vmdl" )]
	public partial class PersonalChest : ModelEntity, ISourceEntity, IResettable
	{
		[Net] public NetInventoryContainer Inventory { get; private set; }

		public virtual void Reset()
		{
			Inventory.Instance.RemoveAll();
		}

		public virtual void Serialize( BinaryWriter writer ) { }

		public virtual void Deserialize( BinaryReader reader ) { }

		public override void Spawn()
		{
			SetModel( "models/editor/playerstart.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );

			var inventory = new InventoryContainer( this );
			inventory.SetSlotLimit( 24 );
			InventorySystem.Register( inventory );

			Inventory = new NetInventoryContainer( inventory );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }
	}
}
