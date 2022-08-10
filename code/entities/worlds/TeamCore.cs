using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Utility;
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
	public partial class TeamCore : ModelEntity, ISourceEntity, IResettable, IHudRenderer
	{
		[EditorProperty, Net] public Team Team { get; set; }
		[Net] public float MaxHealth { get; private set; } = 200f;

		[Net, Change( nameof( OnUpgradeTypesChanged ) )] protected IList<int> UpgradeTypes { get; set; }
		protected List<BaseTeamUpgrade> InternalUpgrades { get; set; } = new();
		public IReadOnlyList<BaseTeamUpgrade> Upgrades => InternalUpgrades;

		private TimeUntil NextAutoHeal { get; set; }
		private Particles Effect { get; set; }

		public TeamCore()
		{
			UpgradeTypes = new List<int>();
		}

		public T FindUpgrade<T>() where T : BaseTeamUpgrade
		{
			return (Upgrades.FirstOrDefault( u => u is T ) as T);
		}

		public void AddUpgrade( BaseTeamUpgrade upgrade )
		{
			Announcements.Send( Team.GetTo(), $"{upgrade.Name} Unlocked", upgrade.Description, upgrade.GetIcon( null ) );
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

		public void Explode()
		{
			Game.RemoveValidTeam( Team );
			CreateDeathEffect();
			LifeState = LifeState.Dead;
		}

		public virtual void Reset()
		{
			Game.AddValidTeam( Team );

			CreateCoreEffect();
			LifeState = LifeState.Alive;
			InternalUpgrades.Clear();
			UpgradeTypes.Clear();
			Health = MaxHealth;
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( (byte)Team );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Team = (Team)reader.ReadByte();
		}

		public virtual void RenderHud( Vector2 screenSize )
		{
			if ( Local.Pawn is not Player player || player.Team != Team )
				return;

			var draw = Render.Draw2D;
			var position = (WorldSpaceBounds.Center + Vector3.Up * 96f).ToScreen();
			var iconSize = 64f;
			var iconAlpha = 1f;

			position.x *= screenSize.x;
			position.y *= screenSize.y;
			position.x -= iconSize * 0.5f;
			position.y -= iconSize * 0.5f;

			var distanceToPawn = Local.Pawn.Position.Distance( Position );

			if ( distanceToPawn <= 1024f )
			{
				iconAlpha = distanceToPawn.Remap( 512f, 1024, 0f, 1f );
			}

			draw.Color = Color.White.WithAlpha( iconAlpha );
			draw.BlendMode = BlendMode.Normal;
			draw.Image( "textures/ui/logo_spinner.png", new Rect( position.x, position.y, iconSize, iconSize ) );
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
			if ( LifeState == LifeState.Alive )
			{
				CreateCoreEffect();
			}
			
			base.ClientSpawn();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( LifeState == LifeState.Dead )
				return;

			if ( !info.Attacker.IsValid() || info.Attacker is not Player attacker )
				return;

			if ( !Game.FriendlyFire && attacker.Team == Team )
				return;

			FloatingDamage.Show( this, info.Damage, info.Position );

			PlaySound( "core.hit" );

			base.TakeDamage( info );
		}

		public override void OnKilled()
		{
			if ( LastAttacker is Player player )
			{
				player.Client.AddInt( "cores", 1 );
			}
			
			Explode();
		}

		public virtual void OnUpgradeTypesChanged( IList<int> oldTypes, IList<int> newTypes )
		{
			InternalUpgrades.Clear();

			foreach ( var index in newTypes )
			{
				var upgrade = TypeLibrary.Create<BaseTeamUpgrade>( index );
				InternalUpgrades.Add( upgrade );
			}
		}

		[ClientRpc]
		protected void CreateCoreEffect()
		{
			Effect?.Destroy();
			Effect = Particles.Create( "particles/gameplay/core/core_crystal/core_crystal.vpcf", this, "Core" );
		}

		[ClientRpc]
		protected void CreateDeathEffect()
		{
			PlaySound( "core.explode1" );

			Effect?.Destroy();
			Effect = Particles.Create( "particles/gameplay/core/core_crystal/core_break/core_crystal_base.vpcf", this, "Core" );
			Effect.SetPosition( 6, Team.GetColor() * 255f );

			PlayExplodeSound();
		}

		protected async void PlayExplodeSound()
		{
			await Task.Delay( 900 );
			PlaySound( "core.explode2" );
		}

		protected override void OnDestroy()
		{
			Effect?.Destroy();
			base.OnDestroy();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( LifeState == LifeState.Alive && NextAutoHeal )
			{
				NextAutoHeal = 0.5f;
				Health = Math.Min( Health + 1f, MaxHealth );
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			Effect?.SetPosition( 6, Team.GetColor() * 255f );
		}
	}
}
