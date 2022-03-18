using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	public interface IDraggable
	{
		float IconSize { get; }
		string GetIconTexture();
	}
}
