using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class PlaceEntityAction : EditorAction
	{
		public override string Name => "Place Entity";

		private TypeDescription EntityType { get; set; }
		private Vector3 Position { get; set; }
		private Rotation Rotation { get; set; }
		private int EntityId { get; set; }

		public void Initialize( TypeDescription type, Vector3 position, Rotation rotation )
		{
			EntityType = type;
			Position = position;
			Rotation = rotation;
		}

		public override void Perform()
		{
			var entity = TypeLibrary.Create<ISourceEntity>( EntityType.Identity );
			entity.Position = Position;
			entity.Rotation = Rotation;

			if ( EntityId > 0 )
				UpdateObject( EntityId, entity );
			else
				EntityId = AddObject( entity );

			base.Perform();
		}

		public override void Undo()
		{
			if ( FindObject<ISourceEntity>( EntityId, out var entity ) && entity.IsValid() )
			{
				entity.Delete();
			}

			base.Undo();
		}
	}
}
