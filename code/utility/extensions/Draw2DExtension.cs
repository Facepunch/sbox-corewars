using Sandbox;

namespace Facepunch.CoreWars.Utility
{
	public static class Draw2DExtension
	{
		/// <summary>
		/// Draw an image to the screen.
		/// </summary>
		public static void Image( this Draw2D self, string texture, Rect rect )
		{
			self.Texture = Texture.Load( FileSystem.Mounted, texture );
			self.Quad( rect, self.Color );
			self.Texture = Texture.White;
		}
	}
}
