using Facepunch.CoreWars.Editor;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Core", EditorModel = "models/gameplay/base_core/base_core.vmdl" )]
	[Category( "Team Entities" )]
	public partial class TeamCore : ModelEntity, ISourceEntity, IResettable
	{
		[EditorProperty, Net] public Team Team { get; set; }

		[Net, Change( nameof( OnUpgradeTypesChanged ) )] protected List<int> UpgradeTypes { get; set; } = new();
		protected List<BaseTeamUpgrade> InternalUpgrades { get; set; } = new();
		public IReadOnlyList<BaseTeamUpgrade> Upgrades => InternalUpgrades;

		private Particles Effect { get; set; }

		public T FindUpgrade<T>() where T : BaseTeamUpgrade
		{
			return (Upgrades.FirstOrDefault( u => u is T ) as T);
		}

		public void AddUpgrade( BaseTeamUpgrade upgrade )
		{
			InternalUpgrades.Add( upgrade );
			UpgradeTypes.Add( TypeLibrary.GetDescription( upgrade.GetType() ).Identity );
		}

		public int GetUpgradeTier( string group )
		{
			var valid = Upgrades.Where( u => u.Group == group );
			if ( !valid.Any() ) return 0;
			return valid.Max( u => u.Tier );
		}

		public bool HasPreviousUpgrade( string group, int tier )
		{
			return Upgrades.Any( u => u.Group == group && u.Tier == tier - 1 );
		}

		public bool HasNewerUpgrade( string group, int tier )
		{
			return Upgrades.Any( u => u.Group == group && u.Tier >= tier );
		}

		public bool HasUpgrade( Type type )
		{
			return Upgrades.Any( u => u.GetType() == type );
		}

		public bool HasUpgrade<T>() where T : BaseTeamUpgrade
		{
			return Upgrades.Any( u => u is T );
		}

		public virtual void Reset()
		{
			Game.AddValidTeam( Team );

			EnableAllCollisions = true;
			EnableDrawing = true;
			LifeState = LifeState.Alive;
			InternalUpgrades.Clear();
			UpgradeTypes.Clear();
			Health = 100f;
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		public override void Spawn()
		{
			SetModel( "models/gameplay/base_core/base_core.vmdl" );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Transmit = TransmitType.Always;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Effect = Particles.Create( "particles/gameplay/core/core_crystal/core_crystal.vpcf", this, "Core" );
			base.ClientSpawn();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( !info.Attacker.IsValid() || info.Attacker is not Player attacker )
				return;

			if ( attacker.Team == Team )
				return;

			base.TakeDamage( info );
		}

		public override void OnKilled()
		{
			Game.RemoveValidTeam( Team );

			EnableAllCollisions = false;
			EnableDrawing = false;
			LifeState = LifeState.Dead;
		}

		public virtual void OnUpgradeTypesChanged( List<int> oldTypes, List<int> newTypes )
		{
			InternalUpgrades.Clear();

			foreach ( var index in newTypes )
			{
				var upgrade = TypeLibrary.Create<BaseTeamUpgrade>( index );
				InternalUpgrades.Add( upgrade );
			}
		}

		protected override void OnDestroy()
		{
			Effect?.Destroy();
			base.OnDestroy();
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			Effect?.SetPosition( 6, Team.GetColor() * 255f );
		}
	}
}
