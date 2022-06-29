using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Reflection;

namespace Facepunch.CoreWars.Editor
{
	public class PlaceEntityAction : EditorAction
	{
		public override string Name => "Place Entity";

		private Dictionary<string, object> Properties { get; set; }
		private TypeDescription EntityType { get; set; }
		private Vector3 Position { get; set; }
		private Rotation Rotation { get; set; }
		private int EntityId { get; set; }

		public void Initialize( TypeDescription type, Vector3 position, Rotation rotation, Dictionary<string,object> properties = null )
		{
			if ( properties != null )
			{
				Properties = new Dictionary<string, object>( properties );
			}

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

			if ( Properties != null )
			{
				UpdateProperties( entity );
			}

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

		private void UpdateProperties( ISourceEntity entity )
		{
			var properties = TypeLibrary.GetProperties( entity );

			foreach ( var property in properties )
			{
				if ( property.GetCustomAttribute<EditorPropertyAttribute>() == null )
				{
					continue;
				}

				if ( Properties.TryGetValue( property.Name, out var value ) )
				{
					property.SetValue( entity, value );

					if ( entity is IEditorCallbacks callbacks )
					{
						callbacks.OnPropertyChanged( property.Name );
					}
				}
			}
		}
	}
}
