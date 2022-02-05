using Facepunch.CoreWars.Voxel;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Hotbar : Panel
	{
		public static Hotbar Current { get; private set; }

		public List<HotbarSlot> Slots { get; private set; }

		public Hotbar()
		{
			Slots = new();
			Current = this;
		}

		public override void Tick()
		{
			if ( Local.Pawn is Player player )
			{
				for ( var i = 0; i < Slots.Count; i++)
				{
					if ( player.HotbarBlocks.Count > i )
						Slots[i].SetBlockId( player.HotbarBlocks[i] );
					else
						Slots[i].SetBlockId( 0 );

					Slots[i].IsSelected = player.CurrentBlockId == Slots[i].BlockId;
				}
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			foreach ( var slot in Slots )
			{
				slot.Delete();
			}

			Slots.Clear();

			for ( var i = 0; i < 8; i++ )
			{
				var slot = AddChild<HotbarSlot>();
				Slots.Add( slot );
			}

			base.PostTemplateApplied();
		}
	}
}
