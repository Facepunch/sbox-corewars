using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EditorToolSelector : Panel
	{
		public static EditorToolSelector Current { get; private set; }

		public List<EditorToolItem> Items { get; private set; }

		public EditorToolSelector()
		{
			Items = new();
			Current = this;
		}

		public void Initialize()
		{
			foreach ( var item in Items )
			{
				item.Delete();
			}

			Items.Clear();

			var available = Library.GetAttributes<EditorToolLibraryAttribute>();

			foreach ( var attribute in available )
			{
				var item = AddChild<EditorToolItem>();
				item.SetLibraryItem( attribute );
				Items.Add( item );
			}
		}

		public override void Tick()
		{
			if ( !VoxelWorld.Current.IsValid() ) return;

			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "hidden", IsHidden );
			Initialize();
			base.PostTemplateApplied();
		}

		private bool IsHidden() => !Input.Down( InputButton.Score );

		[Event.BuildInput]
		private void BuildInput( InputBuilder builder )
		{
			if ( builder.Down( InputButton.Score ) )
			{
				builder.ClearButton( InputButton.Attack1 );
				builder.ClearButton( InputButton.Attack2 );
			}
		}
	}
}
