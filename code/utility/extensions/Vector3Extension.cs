using Sandbox;
using System;

namespace Facepunch.CoreWars.Utility
{
	public static class Vector3Extension
	{
		public static Vector3 RotateAboutAxis( this Vector3 vector, Vector3 axis, float degrees )
		{
			var radians = (MathF.PI / 180f) * degrees;
			var vxp = Vector3.Cross( axis, vector );
			var vxvxp = Vector3.Cross( axis, vxp );
			return vector + MathF.Sin( radians ) * vxp + (1f - MathF.Cos( radians )) * vxvxp;
		}

		public static Vector3 RotateAboutPoint( this Vector3 vector, Vector3 pivot, Vector3 axis, float degrees )
		{
			return pivot + RotateAboutAxis( vector - pivot, axis, degrees );
		}
	}
}
