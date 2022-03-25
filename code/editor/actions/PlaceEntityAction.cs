using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class PlaceEntityAction : EditorAction
	{
		public override string Name => "Place Entity";

		private EditorEntityLibraryAttribute Attribute { get; set; }
		private Vector3 Position { get; set; }
		private Rotation Rotation { get; set; }
		private Entity Entity { get; set; }

		public void Initialize( EditorEntityLibraryAttribute attribute, Vector3 position, Rotation rotation )
		{
			Attribute = attribute;
			Position = position;
			Rotation = rotation;
		}

		public override void Perform()
		{
			Entity = Library.Create<Entity>( Attribute.Name );
			Entity.Position = Position;
			Entity.Rotation = Rotation;

			base.Perform();
		}

		public override void Undo()
		{
			if ( Entity.IsValid() )
			{
				Entity.Delete();
				Entity = null;
			}

			base.Undo();
		}
	}
}
