﻿using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars.UI
{
	public interface ITooltipProvider
	{
		public string Name { get; }
		public string Description { get; }
		public IReadOnlySet<string> Tags { get; }
		public bool IsVisible { get; }
		public Color Color { get; }
		public bool HasHovered { get;  }
	}
}
