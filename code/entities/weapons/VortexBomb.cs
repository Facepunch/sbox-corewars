using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	[Library]
	public class VortexBombConfig : WeaponConfig
	{
		public override string ClassName => "weapon_vortex_bomb";
		public override AmmoType AmmoType => AmmoType.None;
		public override WeaponType Type => WeaponType.Melee;
	}

	[Library( "weapon_vortex_bomb" )]
	public partial class VortexBomb : BlockPlaceWeapon<VortexBombBlock>
	{
		public override WeaponConfig Config => new FireballConfig();
		public override string ViewModelPath => null;
		public override int ViewModelMaterialGroup => 1;

		public override void Spawn()
		{
			base.Spawn();
		}

		public override void AttackPrimary()
		{
			base.AttackPrimary();
		}

		protected override void OnBlockPlaced( IntVector3 position )
		{
			if ( IsServer && Owner is Player player )
			{
				if ( WeaponItem.IsValid() )
				{
					WeaponItem.Remove();
				}
			}

			base.OnBlockPlaced( position );
		}
	}
}
