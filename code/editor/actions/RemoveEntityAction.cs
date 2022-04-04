using Facepunch.Voxels;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars.Editor
{
	public class RemoveEntityAction : EditorAction
	{
		public override string Name => "Remove Entity";

		private EditorEntityAttribute Attribute { get; set; }
		private Transform Transform { get; set; }
		private ISourceEntity Entity { get; set; }
		private byte[] Data { get; set; }
		private int EntityId { get; set; }

		public void Initialize( ISourceEntity entity )
		{
			Attribute = Library.GetAttribute( entity.GetType() ) as EditorEntityAttribute;
			
			if ( FindObjectId( entity, out var id ) )
			{
				EntityId = id;
			}
		}

		public override void Perform()
		{
			if ( !FindObject<ISourceEntity>( EntityId, out var entity ) )
				return;

			if ( !entity.IsValid() )
				return;

			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					entity.Serialize( writer );
				}

				Data = stream.ToArray();
			}

			Transform = entity.Transform;
			entity.Delete();

			base.Perform();
		}

		public override void Undo()
		{
			using ( var stream = new MemoryStream( Data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					var entity = Library.Create<ISourceEntity>( Attribute.Name );
					entity.Transform = Transform;
					entity.Deserialize( reader );
					UpdateObject( EntityId, entity );
				}
			}

			base.Undo();
		}
	}
}
