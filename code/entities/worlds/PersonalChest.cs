using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Personal Chest", EditorModel = "models/gameplay/personal_chest/personal_chest.vmdl" )]
	[Category( "Gameplay" )]
	public partial class PersonalChest : ModelEntity, ISourceEntity, IUsable
	{
		public virtual float MaxUseDistance => 300f;

		public virtual void Serialize( BinaryWriter writer ) { }

		public virtual void Deserialize( BinaryReader reader ) { }

		public override void Spawn()
		{
			SetModel( "models/gameplay/personal_chest/personal_chest.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Static );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }

		public void OnUsed( Player player )
		{
			OpenForClient( To.Single( player ) );
		}

		public bool IsUsable( Player player )
		{
			return true;
		}

		[ClientRpc]
		private void OpenForClient()
		{
			if ( Local.Pawn is not Player player )
				return;

			var storage = Storage.Current;

			storage.SetName( "Personal Chest" );
			storage.SetEntity( this );
			storage.SetContainer( player.ChestInventory.Instance );
			storage.Open();

			Util.Play( "inventory.open" );
		}
	}
}
