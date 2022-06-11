namespace Facepunch.CoreWars
{
	public enum AmmoType
	{
		None = 0,
		Bolt = 1
	}

	public static class AmmoTypeExtension
	{
		public static string GetDescription( this AmmoType self )
		{
			if ( self == AmmoType.Bolt )
			{
				return "Ammo for Crossbow weapons.";
			}

			return string.Empty;
		}
	}
}
