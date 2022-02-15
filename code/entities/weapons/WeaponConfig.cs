namespace Facepunch.CoreWars
{
	public enum WeaponType
	{
		None,
		Hitscan,
		Projectile
	}

	public abstract class WeaponConfig
	{
		public virtual string Name => "";
		public virtual string ClassName => "";
		public virtual string Description => "";
		public virtual string Icon => "";
		public virtual AmmoType AmmoType => AmmoType.Explosive;
		public virtual WeaponType Type => WeaponType.None;
		public virtual int Damage => 0;
		public virtual int Ammo => 0;
	}
}
