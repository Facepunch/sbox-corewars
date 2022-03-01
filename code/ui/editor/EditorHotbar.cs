using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorHotbar : Panel
	{
		public static EditorHotbar Current { get; private set; }

		public List<EditorHotbarSlot> Slots { get; private set; }

		public EditorHotbar()
		{
			Slots = new();
			Current = this;
		}

		public void Initialize( int slots )
		{
			foreach ( var slot in Slots )
			{
				slot.Delete();
			}

			Slots.Clear();

			for ( ushort i = 0; i < slots; i++ )
			{
				var slot = AddChild<EditorHotbarSlot>();
				slot.BlockId = 0;
				slot.Slot = i;
				Slots.Add( slot );
			}
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			if ( Local.Pawn is EditorPlayer player )
			{
				for ( ushort i = 0; i < Slots.Count; i++)
				{
					var blockId = player.HotbarBlockIds[i];

					Slots[i].SetBlockId( blockId );
					Slots[i].IsSelected = player.CurrentHotbarIndex == i;
				}
			}

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			if ( Local.Pawn is EditorPlayer player )
			{
				Initialize( player.HotbarBlockIds.Count );
			}

			base.PostTemplateApplied();
		}
	}
}
