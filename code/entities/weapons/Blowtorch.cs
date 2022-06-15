using Facepunch.CoreWars.Blocks;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class BlowtorchConfig : WeaponConfig
	{
		public override string ClassName => "weapon_blowtorch";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 20;
	}

	[Library( "weapon_blowtorch" )]
	public partial class Blowtorch : BlockDamageWeapon
	{
		public override WeaponConfig Config => new BlowtorchConfig();
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
		public override DamageFlags DamageType => DamageFlags.Burn;
		public override float PrimaryRate => 5f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Plastic;
		public override float SecondaryMaterialMultiplier => 0f;

		private RealTimeUntil DestroyFlameTime { get; set; }
		private Particles FlameParticles { get; set; }
		private Sound? FlameSound { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( IsServer && WeaponItem.IsValid() )
			{
				DamageVoxelInDirection( 80f, Config.Damage );
			}

			if ( FlameParticles == null )
			{
				FlameParticles = Particles.Create( "particles/weapons/blowtorch/blowtorch_flame.vpcf" );
				FlameParticles.SetEntityAttachment( 0, EffectEntity, "muzzle" );
			}

			if ( !FlameSound.HasValue )
			{
				FlameSound = PlaySound( "weapon.blowtorch" );
			}

			DestroyFlameTime = 0.5f;
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
			if ( DestroyFlameTime && FlameParticles != null )
			{
				if ( FlameSound.HasValue )
				{
					FlameSound.Value.Stop();
					FlameSound = null;
				}

				FlameParticles?.Destroy();
				FlameParticles = null;
			}
		}

		protected override void OnDestroy()
		{
			if ( FlameSound.HasValue )
			{
				FlameSound.Value.Stop();
				FlameSound = null;
			}

			FlameParticles?.Destroy();
			base.OnDestroy();
		}
	}
}
