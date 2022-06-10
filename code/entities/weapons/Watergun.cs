using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class WatergunConfig : WeaponConfig
	{
		public override string ClassName => "weapon_watergun";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 30;
	}

	[Library( "weapon_watergun" )]
	public partial class Watergun : BlockDamageWeapon
	{
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Explosives;
		public override float SecondaryMaterialMultiplier => 0f;
		public override WeaponConfig Config => new BlowtorchConfig();
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
		public override DamageFlags DamageType => DamageFlags.Burn;
		public override float PrimaryRate => 5f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;

		private RealTimeUntil DestroySparksTime { get; set; }
		private Particles SparksParticles { get; set; }
		private Sound? SparksSound { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( IsServer && WeaponItem.IsValid() )
			{
				DamageVoxelInDirection( 100f, Config.Damage );
			}

			if ( SparksParticles == null )
			{
				SparksParticles = Particles.Create( "particles/weapons/blowtorch/blowtorch_flame.vpcf" );
				SparksParticles.SetEntityAttachment( 0, EffectEntity, "muzzle" );
			}

			if ( !SparksSound.HasValue )
			{
				SparksSound = PlaySound( "weapon.defuser" );
			}

			DestroySparksTime = 0.5f;
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 2 );
			anim.SetAnimParameter( "aim_body_weight", 1.0f );

			if ( Owner.IsValid() )
			{
				ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
				ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
			}
		}

		[Event.Tick]
		protected virtual void Tick()
		{
			if ( DestroySparksTime && SparksParticles != null )
			{
				if ( SparksSound.HasValue )
				{
					SparksSound.Value.Stop();
					SparksSound = null;
				}

				SparksParticles?.Destroy();
				SparksParticles = null;
			}
		}

		protected override void OnBlockDestroyed( IntVector3 position )
		{
			using ( Prediction.Off() )
			{
				PlaySound( "defuser.success" );
			}
		}

		protected override void OnDestroy()
		{
			if ( SparksSound.HasValue )
			{
				SparksSound.Value.Stop();
				SparksSound = null;
			}

			SparksParticles?.Destroy();
			base.OnDestroy();
		}
	}
}
