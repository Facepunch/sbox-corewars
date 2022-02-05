using Facepunch.CoreWars.Voxel;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class HotbarSlot : Panel
	{
		public bool IsSelected { get; set; }
		public byte BlockId { get; set; }

		public HotbarSlot() { }

		public void SetBlockId( byte blockId )
		{
			BlockId = blockId;

			if ( Map.Current == null ) return;

			var block = Map.Current.GetBlockType( blockId );
			var texture = Texture.Load( FileSystem.Mounted, $"textures/blocks/{ block.DefaultTexture }.png" );
			Style.SetBackgroundImage( texture );
		}

		protected override void PostTemplateApplied()
		{
			SetBlockId( BlockId );
			BindClass( "selected", () => IsSelected );
			base.PostTemplateApplied();
		}
	}
}
