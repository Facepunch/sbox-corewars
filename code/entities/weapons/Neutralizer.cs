using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class NeutralizerConfig : WeaponConfig
	{
		public override string ClassName => "weapon_neutralizer";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 30;
	}

	[Library( "weapon_neutralizer" )]
	public partial class Neutralizer : BlockDamageWeapon
	{
		public override BuildingMaterialType PrimaryMaterialType => BuildingMaterialType.Explosives;
		public override float SecondaryMaterialMultiplier => 0f;
		public override WeaponConfig Config => new NeutralizerConfig();
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";
		public override DamageFlags DamageType => DamageFlags.Burn;
		public override float PrimaryRate => 5f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;

		private RealTimeUntil DestroyEffectTime { get; set; }
		private Particles ParticleEffect { get; set; }
		private Sound? EffectSound { get; set; }

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

			if ( ParticleEffect == null )
			{
				ParticleEffect = Particles.Create( "particles/weapons/neutralizer/neutralizer_flame.vpcf" );
				ParticleEffect.SetEntityAttachment( 0, EffectEntity, "muzzle" );
			}

			if ( !EffectSound.HasValue )
			{
				EffectSound = PlaySound( "weapon.defuser" );
			}

			DestroyEffectTime = 0.5f;
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
			if ( DestroyEffectTime && ParticleEffect != null )
			{
				if ( EffectSound.HasValue )
				{
					EffectSound.Value.Stop();
					EffectSound = null;
				}

				ParticleEffect?.Destroy();
				ParticleEffect = null;
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
			if ( EffectSound.HasValue )
			{
				EffectSound.Value.Stop();
				EffectSound = null;
			}

			ParticleEffect?.Destroy();
			base.OnDestroy();
		}
	}
}
