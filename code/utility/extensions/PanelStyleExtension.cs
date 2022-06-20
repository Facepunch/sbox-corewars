﻿using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars.Utility
{
	public static class PanelStyleExtension
	{
		/// <summary>
		/// Set the background to a linear gradient.
		/// </summary>
		public static void SetLinearGradientBackground( this PanelStyle self, Color from, float fromOpacity, Color to, float toOpacity )
		{
			self.Set( "background", $"linear-gradient(rgba({from.Hex}, {fromOpacity}), rgba({to.Hex}, {toOpacity}))" );
		}
	}
}