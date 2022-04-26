using Facepunch.CoreWars.Editor;
using System.Collections.Generic;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Team Core", Group = "Team Entities", EditorModel = "models/gameplay/base_core/base_core.vmdl" )]
	public partial class TeamCore : ModelEntity, ISourceEntity, IResettable
	{
		[EditorProperty, Net] public Team Team { get; set; }

		[Net] public List<BaseTeamUpgrade> Upgrades { get; set; }

		public virtual void Reset()
		{
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
			SetupPhysicsFromAABB( PhysicsMotionType.Static, Model.Bounds.Mins, Model.Bounds.Maxs );

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
			LifeState = LifeState.Dead;
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			RenderColor = Team.GetColor();
		}
	}
}
