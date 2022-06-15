using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public static class ColorPalette
	{
		public static Color Weapons { get; private set; } = new Color( 0xa94949 );
		public static Color Armor { get; private set; } = new Color( 0x5c699f );
		public static Color Brews { get; private set; } = new Color( 0xe8c65b );
		public static Color Ammo { get; private set; } = new Color( 0xbdaa97 );
		public static Color Resources { get; private set; } = new Color( 0x8bb0ad );
		public static Color Abilities { get; private set; } = new Color( 0xeeb551 );
		public static Color Tools { get; private set; } = new Color( 0x3e554c );
	}
}
