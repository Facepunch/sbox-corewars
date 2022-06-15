using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordConfig : WeaponConfig
	{
		public override string ClassName => "weapon_sword";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
		public override int Ammo => 0;
		public override int Damage => 15;
	}

	[Library( "weapon_sword" )]
	public partial class Sword : MeleeWeapon
	{
		public override WeaponConfig Config => new SwordConfig();
		public override string[] KillFeedReasons => new[] { "chopped", "slashed" };
		public override string ViewModelPath => "models/weapons/sword/v_sword01.vmdl";
		public override DamageFlags DamageType => DamageFlags.Blunt;
		public override float MeleeRange => 80f;
		public override float PrimaryRate => 1.5f;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/sword/w_sword01.vmdl" );
		}
	}
}
