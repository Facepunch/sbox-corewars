﻿using Facepunch.CoreWars.Utility;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public interface IConsumableItem
	{
		public void Consume( CoreWarsPlayer player );
	}
}
