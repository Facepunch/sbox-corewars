using Facepunch.CoreWars.Editor;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Upgrades NPC", EditorModel = "models/citizen/citizen.vmdl" )]
	[Category( "Gameplay" )]
	public partial class TeamUpgradesNPC : AnimatedEntity, ISourceEntity, IUsable, INameplate
	{
		[EditorProperty] public Team Team { get; set; }

		public List<BaseTeamUpgrade> Upgrades { get; private set; } = new();

		public string DisplayName => "Team Upgrades";
		public float MaxUseDistance => 300f;
		public bool IsFriendly => true;

		private Nameplate Nameplate { get; set; }

		public override void Spawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;

			AddClothing( "models/citizen_clothes/shirt/chainmail/models/chainmail.vmdl" );
			AddClothing( "models/citizen_clothes/trousers/legarmour/models/leg_armour.vmdl" );
			AddClothing( "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.vmdl" );
			AddClothing( "models/citizen_clothes/shoes/trainers/trainers.vmdl" );
			AddClothing( "models/citizen_clothes/glasses/stylish_glasses/models/stylish_glasses_gold.vmdl" );
			AddClothing( "models/citizen_clothes/hair/hair_longbrown/models/hair_longbrown.vmdl" );

			AddAllUpgrades();

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Nameplate = new Nameplate( this );

			AddAllUpgrades();

			base.ClientSpawn();
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		public void AddClothing( string modelName )
		{
			var clothes = new BaseClothing();
			clothes.SetModel( modelName );
			clothes.SetParent( this, true );
		}

		public bool IsUsable( Player player )
		{
			return true;
		}

		public void OnUsed( Player player )
		{
			OpenForClient( To.Single( player ) );
		}

		private void AddAllUpgrades()
		{
			var types = TypeLibrary.GetTypes<BaseTeamUpgrade>();

			foreach ( var type in types )
			{
				if ( type.IsAbstract || type.IsGenericType ) continue;
				var upgrade = TypeLibrary.Create<BaseTeamUpgrade>( type );
				Upgrades.Add( upgrade );
			}
		}

		[ClientRpc]
		private void OpenForClient()
		{
			UpgradeStore.Current.SetNPC( this );
			UpgradeStore.Current.Open();
		}
	}
}
