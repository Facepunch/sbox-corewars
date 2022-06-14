using Sandbox;

namespace Facepunch.CoreWars.Utility
{
	public static class Render2DExtension
	{
		/// <summary>
		/// Draw an image to the screen.
		/// </summary>
		public static void Image( this Sandbox.Render.Render2D self, string texture, Rect rect )
		{
			self.Material = null;
			self.Texture = Texture.Load( FileSystem.Mounted, texture );
			self.Quad( rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft );
		}
	}
}
