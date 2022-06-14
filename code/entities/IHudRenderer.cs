using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public interface IHudRenderer : IValid
	{
		public void RenderHud( Vector2 screenSize );
	}
}
