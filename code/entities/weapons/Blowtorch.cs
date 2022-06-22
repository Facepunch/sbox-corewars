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

		private RealTimeUntil DestroyEffectTime { get; set; }
		private Particles ParticleEffect { get; set; }
		private Sound? SoundEffect { get; set; }

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
				ParticleEffect = Particles.Create( "particles/weapons/blowtorch/blowtorch_flame.vpcf" );
				ParticleEffect.SetEntityAttachment( 0, EffectEntity, "muzzle" );
			}

			if ( !SoundEffect.HasValue )
			{
				SoundEffect = PlaySound( "weapon.blowtorch" );
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
				if ( SoundEffect.HasValue )
				{
					SoundEffect.Value.Stop();
					SoundEffect = null;
				}

				ParticleEffect?.Destroy();
				ParticleEffect = null;
			}
		}

		protected override void OnDestroy()
		{
			if ( SoundEffect.HasValue )
			{
				SoundEffect.Value.Stop();
				SoundEffect = null;
			}

			ParticleEffect?.Destroy();
			base.OnDestroy();
		}
	}
}
