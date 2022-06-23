using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public static partial class Util
	{
		public static Vector3 GetPositionMaxs( Vector3 mins, Vector3 maxs )
		{
			var maxX = MathF.Max( mins.x, maxs.x );
			var maxY = MathF.Max( mins.y, maxs.y );
			var maxZ = MathF.Max( mins.z, maxs.z );

			return new Vector3( maxX, maxY, maxZ );
		}

		public static Vector3 GetPositionMins( Vector3 mins, Vector3 maxs )
		{
			var minX = MathF.Min( mins.x, maxs.x );
			var minY = MathF.Min( mins.y, maxs.y );
			var minZ = MathF.Min( mins.z, maxs.z );

			return new Vector3( minX, minY, minZ );
		}
	}
}
