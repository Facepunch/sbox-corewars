using Facepunch.CoreWars.Editor;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Item Store NPC", EditorModel = "models/gameplay/temp/team_shrines/team_shrine.vmdl" )]
	[Category( "Gameplay" )]
	[Alias( "ItemStoreNPC" )]
	public partial class ItemStoreEntity : AnimatedEntity, ISourceEntity, IUsable, INameplate, IItemStore
	{
		[EditorProperty] public Team Team { get; set; }

		public List<BaseShopItem> Items { get; private set; } = new();

		public string DisplayName => "Item Store";
		public float MaxUseDistance => 300f;
		public bool IsFriendly => true;

		private Nameplate Nameplate { get; set; }

		public override void Spawn()
		{
			SetModel( "models/gameplay/temp/team_shrines/team_shrine.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;

			AddAllItems();

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Nameplate = new Nameplate( this );
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
				VoxelWorld.RegisterVoxelModel( this );
				Nameplate?.Delete();
			}

			base.OnDestroy();
		}

		private void AddAllItems()
		{
			var types = TypeLibrary.GetTypes<BaseShopItem>();

			foreach ( var type in types )
			{
				if ( type.IsAbstract || type.IsGenericType ) continue;
				var item = TypeLibrary.Create<BaseShopItem>( type );
				Items.Add( item );
			}
		}

		[ClientRpc]
		private void OpenForClient()
		{
			ItemStore.Current.SetEntity( this );
			ItemStore.Current.Open();
		}
	}
}
