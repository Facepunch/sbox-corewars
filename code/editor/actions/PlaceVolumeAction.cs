using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class PlaceVolumeAction : EditorAction
	{
		public override string Name => "Place Volume";

		private EditorEntityLibraryAttribute Attribute { get; set; }
		private Vector3 Mins { get; set; }
		private Vector3 Maxs { get; set; }
		private Entity Entity { get; set; }

		public void Initialize( EditorEntityLibraryAttribute attribute, Vector3 mins, Vector3 maxs )
		{
			Attribute = attribute;
			Mins = mins;
			Maxs = maxs;
		}

		public override void Perform()
		{
			var volume = Library.Create<IVolumeEntity>( Attribute.Name );

			volume.Position = Mins;
			volume.SetVolume( Mins, Maxs );

			Entity = (volume as Entity);

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
