using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordConfig : WeaponConfig
	{
		public override string Name => "Sword";
		public override string Description => "Your every day melee weapon";
		public override string Icon => "textures/items/weapon_sword_1.png";
		public override string ClassName => "weapon_sword";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 5;
	}

	[Library( "weapon_sword", Title = "Sword" )]
	public partial class Sword : Weapon
	{
		public override WeaponConfig Config => new SwordConfig();
		public override string[] KillFeedReasons => new[] { "chopped", "slashed" };
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override DamageFlags DamageType => DamageFlags.Blunt;
		public override float PrimaryRate => 1.5f;
		public override float SecondaryRate => 1f;
		public override int ClipSize => 0;
		public override bool IsMelee => true;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/sword/w_sword01.vmdl" );
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( WeaponItem.IsValid() )
			{
				if ( !string.IsNullOrEmpty( WeaponItem.ViewModelPath ) )
				{
					ViewModelEntity = new ViewModel
					{
						EnableViewmodelRendering = true,
						Position = Position,
						Owner = Owner
					};

					ViewModelEntity.SetModel( WeaponItem.ViewModelPath );
					ViewModelEntity.SetMaterialGroup( WeaponItem.ViewModelMaterialGroup );

					return;
				}
			}

			base.CreateViewModel();
		}

		public override void AttackPrimary()
		{
			if ( Owner is not Player player )
				return;

			if ( player.IsOutOfBreath )
				return;

			PlayAttackAnimation();
			ShootEffects();
			MeleeStrike( Config.Damage * WeaponItem.Tier, 1.5f * WeaponItem.Tier );
			PlaySound( "melee.swing" );

			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			player.ReduceStamina( 5f );
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

		protected override void OnWeaponItemChanged()
		{
			if ( IsServer && WeaponItem.IsValid() && !string.IsNullOrEmpty( WeaponItem.WorldModelPath ) )
			{
				SetModel( WeaponItem.WorldModelPath );
			}

			base.OnWeaponItemChanged();
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

			if ( victim is Player target )
				target.PlaySound( "melee.hitflesh" );

			base.OnMeleeAttackHit( victim );
		}
	}
}
