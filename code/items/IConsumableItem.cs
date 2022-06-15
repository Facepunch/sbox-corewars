using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Utility;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public interface IConsumableItem
	{
		public void Consume( Player player );
	}
}
