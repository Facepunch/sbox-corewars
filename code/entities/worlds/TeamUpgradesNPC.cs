﻿using Facepunch.CoreWars.Editor;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Upgrades NPC", Group = "Gameplay", EditorModel = "models/citizen/citizen.vmdl" )]
	public partial class TeamUpgradesNPC : AnimEntity, ISourceEntity, IUsable
	{
		[EditorProperty] public Team Team { get; set; }

		[Net] public List<BaseTeamUpgrade> Upgrades { get; set; }

		public float MaxUseDistance => 300f;

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

			Upgrades = new List<BaseTeamUpgrade>();

			var types = Library.GetAll<BaseTeamUpgrade>();

			foreach ( var type in types )
			{
				if ( type.IsAbstract || type.IsGenericType ) continue;
				var upgrade = Library.Create<BaseTeamUpgrade>( type );
				Upgrades.Add( upgrade );
			}

			base.Spawn();
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

		}
	}
}