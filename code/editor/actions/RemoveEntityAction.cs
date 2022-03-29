using Facepunch.Voxels;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars.Editor
{
	public class RemoveEntityAction : EditorAction
	{
		public override string Name => "Remove Entity";

		private EditorEntityLibraryAttribute Attribute { get; set; }
		private Transform Transform { get; set; }
		private ISourceEntity Entity { get; set; }
		private byte[] Data { get; set; }

		public void Initialize( ISourceEntity entity )
		{
			Attribute = Library.GetAttribute( entity.GetType() ) as EditorEntityLibraryAttribute;
			Entity = entity;
		}

		public override void Perform()
		{
			var entity = (Entity as Entity);

			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					Entity.Serialize( writer );
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
					Entity = Library.Create<ISourceEntity>( Attribute.Name );
					Entity.Transform = Transform;
					Entity.Deserialize( reader );
				}
			}

			base.Undo();
		}
	}
}
