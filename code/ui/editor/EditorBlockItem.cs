using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorBlockItem : Panel, IDraggable
	{
		public byte BlockId { get; set; }
		public float IconSize => Box.Rect.Size.Length;

		public EditorBlockItem() { }

		public void SetBlockId( byte blockId )
		{
			BlockId = blockId;

			var block = VoxelWorld.Current.GetBlockType( blockId );
			var icon = $"textures/blocks/color/{ block.DefaultTexture }.png";

			if ( !string.IsNullOrEmpty( icon ) )
			{
				Style.SetBackgroundImage( icon );
				Style.BackgroundSizeX = Length.Cover;
				Style.BackgroundSizeY = Length.Cover;
			}
			else
			{
				Style.BackgroundImage = null;
			}
		}

		protected override void OnMouseDown( MousePanelEvent e )
		{
			Draggable.Start( this, DraggableMode.Move );
			base.OnMouseDown( e );
		}

		protected override void OnMouseUp( MousePanelEvent e )
		{
			Draggable.Stop( this );
			base.OnMouseUp( e );
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();
		}

		public string GetIconTexture()
		{
			var block = VoxelWorld.Current.GetBlockType( BlockId );
			return $"textures/blocks/color/{ block.DefaultTexture }.png";
		}
	}
}
