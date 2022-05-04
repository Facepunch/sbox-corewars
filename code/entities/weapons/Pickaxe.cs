using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PickaxeConfig : WeaponConfig
	{
		public override string Name => "Pickaxe";
		public override string Description => "For breaking tougher blocks";
		public override string Icon => "items/weapon_pickaxe.png";
		public override string ClassName => "weapon_pickaxe";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 10;
	}

	[Library( "weapon_pickaxe", Title = "Pickaxe" )]
	public partial class Pickaxe : Weapon
	{
		public override WeaponConfig Config => new PickaxeConfig();
		public override string ViewModelPath => "models/weapons/v_crowbar.vmdl";
		public override DamageFlags DamageType => DamageFlags.Blunt;
		public override float PrimaryRate => 2f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 1;
		public override bool IsMelee => true;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/w_crowbar.vmdl" );
		}

		public override void AttackPrimary()
		{
			PlayAttackAnimation();
			ShootEffects();
			PlaySound( $"barage.launch" );
			MeleeStrike( Config.Damage, 1.5f );

			if ( IsServer )
			{
				var world = VoxelWorld.Current;
				
				if ( world.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position, 150f ) )
				{
					var voxel = world.GetVoxel( position );

					if ( voxel.IsValid )
					{
						Log.Info( "Hit Voxel" );

						var state = world.GetState<BlockState>( position );
						
						if ( !state.IsValid() )
						{
							state = world.GetOrCreateState<BlockState>( position );
							state.Health = 100;
						}

						state.Health -= 10;
						state.IsDirty = true;

						Log.Info( state.Health );

						if ( state.Health <= 0 )
						{
							world.SetBlockOnServer( position, 0 );
						}
					}
				}
			}

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 5 );
			anim.SetAnimParameter( "aim_body_weight", 1.0f );

			if ( Owner.IsValid() )
			{
				ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
				ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
			}
		}

		protected override void ShootEffects()
		{
			base.ShootEffects();

			ViewModelEntity?.SetAnimParameter( "attack", true );
			ViewModelEntity?.SetAnimParameter( "holdtype_attack", 1 );
		}

		protected override void OnMeleeAttackHit( Entity victim )
		{
			ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
			base.OnMeleeAttackHit( victim );
		}
	}
}
