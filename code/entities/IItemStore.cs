using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public interface IItemStore : IValid
	{
		public float MaxUseDistance { get; }
		public Vector3 Position { get; }
		public List<BaseShopItem> Items { get; }
	}
}
