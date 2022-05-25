using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class MoveEntityAction : EditorAction
	{
		public override string Name => "Move Entity";

		private Transform OldTransform { get; set; }
		private Vector3 Position { get; set; }
		private Rotation Rotation { get; set; }
		private int EntityId { get; set; }

		public void Initialize( Entity entity, Vector3 position, Rotation rotation )
		{
			if ( FindObjectId( entity, out var id ) )
				EntityId = id;
			else
				EntityId = AddObject( entity );

			Position = position;
			Rotation = rotation;
		}

		public override void Perform()
		{
			if ( !FindObject<ISourceEntity>( EntityId, out var entity ) )
				return;

			if ( !entity.IsValid() )
				return;

			OldTransform = entity.Transform;

			entity.Position = Position;
			entity.Rotation = Rotation;
			entity.ResetInterpolation();

			base.Perform();
		}

		public override void Undo()
		{
			if ( FindObject<ISourceEntity>( EntityId, out var entity ) && entity.IsValid() )
			{
				entity.Transform = OldTransform;
			}

			base.Undo();
		}
	}
}
