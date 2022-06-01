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

		[Net] public List<BaseTeamUpgrade> Upgrades { get; set; }

		public T FindUpgrade<T>() where T : BaseTeamUpgrade
		{
			return (Upgrades.FirstOrDefault( u => u is T ) as T);
		}

		public int GetUpgradeTier( string group )
		{
			var valid = Upgrades.Where( u => u.Group == group );
			if ( !valid.Any() ) return 0;
			return valid.Max( u => u.Tier );
		}

		public bool HasUpgrade<T>() where T : BaseTeamUpgrade
		{
			return Upgrades.Any( u => u is T );
		}

		public virtual void Reset()
		{
			Game.AddValidTeam( Team );
			LifeState = LifeState.Alive;
			Upgrades.Clear();
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

			Transmit = TransmitType.Always;
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

			Upgrades = new List<BaseTeamUpgrade>();

			base.Spawn();
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
			LifeState = LifeState.Dead;
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			RenderColor = Team.GetColor();
		}
	}
}
