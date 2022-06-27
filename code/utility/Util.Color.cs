using Sandbox;
using System;
using System.Globalization;

namespace Facepunch.CoreWars
{
	public static partial class Util
	{
		public static uint ColorToInt( Color color )
		{
			var red = (color.r * 255f).FloorToInt();
			var green = (color.g * 255f).FloorToInt();
			var blue = (color.b * 255f).FloorToInt();
			return (uint)(((red & 0x0ff) << 16) | ((green & 0x0ff) << 8) | (blue & 0x0ff));
		}

		public static Color HexToColor( string hex )
		{
			if ( hex.Contains( '#' ) )
				hex = hex.Replace( "#", "" );

			if ( hex.Length == 6 )
			{
				var r = int.Parse( hex.Substring( 0, 2 ), NumberStyles.AllowHexSpecifier ) / 255f;
				var g = int.Parse( hex.Substring( 2, 2 ), NumberStyles.AllowHexSpecifier ) / 255f;
				var b = int.Parse( hex.Substring( 4, 2 ), NumberStyles.AllowHexSpecifier ) / 255f;
				return new Color( r, g, b );
			}
			else
			{
				throw new Exception( "Tried to parse a hex color with less or more than 6 characters!" );
			}
		}
	}
}
