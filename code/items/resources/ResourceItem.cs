using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ResourceItem : InventoryItem
	{
		public override bool DropOnDeath => true;
		public override Color Color => UI.ColorPalette.Resources;
	}
}
