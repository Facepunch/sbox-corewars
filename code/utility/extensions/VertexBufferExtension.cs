using Sandbox;

namespace Facepunch.CoreWars.Utility
{
	public static class VertexBufferExtension
	{
		/// <summary>
		/// Add a quad to the vertex buffer. Will include indices if they're enabled.
		/// </summary>
		public static void AddQuad( this VertexBuffer self, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 uvScale )
		{
			if ( self.Indexed )
			{
				self.Add( a, new Vector2( 0, 0 ) );
				self.Add( b, new Vector2( uvScale.x, 0 ) );
				self.Add( c, new Vector2( uvScale.x, uvScale.y ) );
				self.Add( d, new Vector2( 0, uvScale.y ) );

				self.AddTriangleIndex( 4, 3, 2 );
				self.AddTriangleIndex( 2, 1, 4 );
			}
			else
			{
				self.Add( a, new Vector2( 0, 0 ) );
				self.Add( b, new Vector2( uvScale.x, 0 ) );
				self.Add( c, new Vector2( uvScale.x, uvScale.y ) );

				self.Add( c, new Vector2( uvScale.x, uvScale.y ) );
				self.Add( d, new Vector2( 0, uvScale.y ) );
				self.Add( a, new Vector2( 0, 0 ) );
			}
		}

		/// <summary>
		/// Add a quad to the vertex buffer. Will include indices if they're enabled.
		/// </summary>
		public static void AddQuad( this VertexBuffer self, Ray origin, Vector3 width, Vector3 height, Vector2 uvScale )
		{
			self.Default.Normal = origin.Direction;
			self.Default.Tangent = new Vector4( width.Normal, 1 );

			AddQuad( self, origin.Origin - width - height,
				origin.Origin + width - height,
				origin.Origin + width + height,
				origin.Origin - width + height,
				uvScale );
		}

		/// <summary>
		/// Add a cube to the vertex buffer. Will include indices if they're enabled.
		/// </summary>
		public static void AddCube( this VertexBuffer self, Vector3 center, Vector3 size, Rotation rot, Color32 color, Vector3 uvScale, bool inverted = false )
		{
			var oldColor = self.Default.Color;
			self.Default.Color = color;

			var f = rot.Forward * size.x * 0.5f;
			var l = rot.Left * size.y * 0.5f;
			var u = rot.Up * size.z * 0.5f;

			if ( inverted )
			{
				f = rot.Backward * size.x * 0.5f;
				l = rot.Right * size.y * 0.5f;
				u = rot.Down * size.z * 0.5f;
			}

			// Forward/Back
			AddQuad( self, new Ray( center + f, f.Normal ), l, u, new Vector2( uvScale.y, uvScale.z ) );
			AddQuad( self, new Ray( center - f, -f.Normal ), l, -u, new Vector2( uvScale.y, uvScale.z ) );

			// Left/Right
			AddQuad( self, new Ray( center + l, l.Normal ), -f, u, new Vector2( uvScale.x, uvScale.z ) );
			AddQuad( self, new Ray( center - l, -l.Normal ), f, u, new Vector2( uvScale.x, uvScale.z ) );

			// Top/Bottom
			AddQuad( self, new Ray( center + u, u.Normal ), f, l, new Vector2( uvScale.x, uvScale.y ) );
			AddQuad( self, new Ray( center - u, -u.Normal ), f, -l, new Vector2( uvScale.x, uvScale.y ) );

			self.Default.Color = oldColor;
		}
	}
}
