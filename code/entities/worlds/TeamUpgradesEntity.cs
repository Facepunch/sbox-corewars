using Facepunch.CoreWars.Editor;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Upgrades", EditorModel = "models/gameplay/temp/team_shrines/team_shrine.vmdl" )]
	[Category( "Gameplay" )]
	[Alias( "TeamUpgradesNPC" )]
	public partial class TeamUpgradesEntity : AnimatedEntity, ISourceEntity, IUsable, INameplate
	{
		[EditorProperty] public Team Team { get; set; }

		public List<BaseTeamUpgrade> Upgrades { get; private set; } = new();

		public string DisplayName => "Team Upgrades";
		public float MaxUseDistance => 300f;
		public bool IsFriendly => true;

		private Nameplate Nameplate { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/temp/team_shrines/team_shrine.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;

			AddAllUpgrades();

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			VoxelWorld.RegisterVoxelModel( this );
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

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				VoxelWorld.UnregisterVoxelModel( this );
				Nameplate?.Delete();
			}

			base.OnDestroy();
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
			UpgradeStore.Current.SetEntity( this );
			UpgradeStore.Current.Open();
		}
	}
}
