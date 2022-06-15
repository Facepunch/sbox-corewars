using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public static class ColorPalette
	{
		public static Color Weapons { get; private set; } = new Color( 169 / 255f, 73 / 255f, 73 / 255f );
		public static Color Armor { get; private set; } = new Color( 92 / 255f, 105 / 255f, 159 / 255f );
		public static Color Brews { get; private set; } = new Color( 232 / 255f, 198 / 255f, 91 / 255f );
		public static Color Ammo { get; private set; } = new Color( 139 / 255f, 145 / 255f, 80 / 255f );
		public static Color Resources { get; private set; } = new Color( 139 / 255f, 176 / 255f, 173 / 255f );
		public static Color Abilities { get; private set; } = new Color( 238 / 255f, 181 / 255f, 81 / 255f );
		public static Color Tools { get; private set; } = new Color( 139 / 255f, 176 / 255f, 173 / 255f );
		public static Color Blocks { get; private set; } = new Color( 217 / 255f, 166 / 255f, 166 / 255f );
	}
}
