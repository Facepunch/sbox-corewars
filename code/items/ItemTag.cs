using System;
using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	public struct ItemTag
	{
		public static ItemTag CanDrop { get; private set; } = new ItemTag( "Droppable", Color.Yellow );
		public static ItemTag Soulbound { get; private set; } = new ItemTag( "Soulbound", Color.Green );
		public static ItemTag UsesStamina { get; private set; } = new ItemTag( "Uses Stamina", Color.Blue );

		public string Name { get; set; }
		public Color Color { get; set; }

		public ItemTag( string name, Color color )
		{
			Name = name;
			Color = color;
		}
	}
}
