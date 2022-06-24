using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorHotbar : Panel
	{
		public static EditorHotbar Current { get; private set; }

		public List<EditorHotbarSlot> Slots { get; private set; }
		public Label CurrentBlockLabel { get; private set; }

		public EditorHotbar()
		{
			Current = this;
		}

		public void Initialize( int slots )
		{
			Slots ??= new();

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
				CurrentBlockLabel.Text = string.Empty;

				for ( ushort i = 0; i < Slots.Count; i++)
				{
					var blockId = player.HotbarBlockIds[i];

					Slots[i].SetBlockId( blockId );
					Slots[i].IsSelected = player.CurrentHotbarIndex == i;

					if ( Slots[i].IsSelected && Slots[i].BlockType.IsValid() )
					{
						CurrentBlockLabel.Text = Slots[i].BlockType.FriendlyName;
					}
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

			CurrentBlockLabel = Add.Label( "", "current-block" );

			base.PostTemplateApplied();
		}
	}
}
