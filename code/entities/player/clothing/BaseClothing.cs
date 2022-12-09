using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class BaseClothing : ModelEntity
	{
		public CoreWarsPlayer Wearer => Parent as CoreWarsPlayer;

		public virtual void Attached() { }

		public virtual void Detatched() { }
	}
}
