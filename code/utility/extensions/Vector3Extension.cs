using Sandbox;
using System;

namespace Facepunch.CoreWars.Utility
{
	public static class Vector3Extension
	{
		public static Vector3 RotateAboutAxis( this Vector3 vector, Vector3 axis, float angle )
		{
			angle = (MathF.PI / 180f) * angle;
			var vxp = Vector3.Cross( axis, vector );
			var vxvxp = Vector3.Cross( axis, vxp );
			return vector + MathF.Sin( angle ) * vxp + (1 - MathF.Cos( angle )) * vxvxp;
		}

		public static Vector3 RotateAboutPoint( this Vector3 vector, Vector3 pivot, Vector3 axis, float angle )
		{
			return pivot + RotateAboutAxis( vector - pivot, axis, angle );
		}
	}
}
