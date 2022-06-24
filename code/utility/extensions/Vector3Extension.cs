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

		public static Vector3 SnapToGridCeiling( this Vector3 vector, float gridSize )
		{
			return new Vector3(
				MathF.Ceiling( vector.x / gridSize ) * gridSize,
				MathF.Ceiling( vector.y / gridSize ) * gridSize,
				MathF.Ceiling( vector.z / gridSize ) * gridSize
			);
		}

		public static Vector3 SnapToGridFloor( this Vector3 vector, float gridSize )
		{
			return new Vector3(
				MathF.Floor( vector.x / gridSize ) * gridSize,
				MathF.Floor( vector.y / gridSize ) * gridSize,
				MathF.Floor( vector.z / gridSize ) * gridSize
			);
		}
	}
}
