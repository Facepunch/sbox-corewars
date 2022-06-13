﻿using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	public interface ITooltipProvider
	{
		public string Name { get; }
		public string Description { get; }
		public bool IsVisible { get; }
	}
}