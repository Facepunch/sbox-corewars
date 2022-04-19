using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Chest", Group = "Team Entities", EditorModel = "models/gameplay/team_chest/team_chest.vmdl" )]
	public partial class TeamChest : ModelEntity, ISourceEntity, IResettable
	{
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
			Inventory.Instance.RemoveAll();
		}

		public override void Spawn()
		{
			SetModel( "models/gameplay/team_chest/team_chest.vmdl" );

			Transmit = TransmitType.Always;
			SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );

			var inventory = new InventoryContainer( this );
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
	}
}
