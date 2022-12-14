using Facepunch.CoreWars.Editor;

using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Chest",  EditorModel = "models/gameplay/team_chest/team_chest.vmdl" )]
	[Category( "Team Entities" )]
	public partial class TeamChest : ModelEntity, ISourceEntity, IResettable, IUsable
	{
		public float MaxUseDistance => 300f;

		[EditorProperty, Net] public Team Team { get; set; }

		[Net] public NetInventoryContainer Inventory { get; private set; }

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		public virtual void Reset()
		{
			Inventory.Value.RemoveAll();
		}

		public override void Spawn()
		{
			SetModel( "models/gameplay/team_chest/team_chest.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			var inventory = new InventoryContainer();
			inventory.SetEntity( this );
			inventory.SetSlotLimit( 24 );
			InventorySystem.Register( inventory );

			Inventory = new NetInventoryContainer( inventory );

			base.Spawn();
		}

		public override void TakeDamage( DamageInfo info ) { }


		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			RenderColor = Team.GetColor();
		}

		public void OnUsed( CoreWarsPlayer player )
		{
			Inventory.Value.AddConnection( player.Client );
			OpenForClient( To.Single( player ), Inventory.Value.Serialize() );
		}

		public bool IsUsable( CoreWarsPlayer player  )
		{
			return player.Team == Team;
		}

		[ClientRpc]
		private void OpenForClient( byte[] data )
		{
			if ( Game.LocalPawn is not CoreWarsPlayer ) return;

			var container = InventoryContainer.Deserialize( data );
			var storage = UI.Storage.Current;

			storage.SetName( "Team Chest" );
			storage.SetEntity( this );
			storage.SetContainer( container );
			storage.Open();

			Util.Play( "inventory.open" );
		}
	}
}
