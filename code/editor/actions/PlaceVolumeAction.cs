﻿using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class PlaceVolumeAction : EditorAction
	{
		public override string Name => "Place Volume";

		private TypeDescription EntityType { get; set; }
		private Vector3 Mins { get; set; }
		private Vector3 Maxs { get; set; }
		private int EntityId  { get; set; }

		public void Initialize( TypeDescription type, Vector3 mins, Vector3 maxs )
		{
			EntityType = type;
			Mins = mins;
			Maxs = maxs;
		}

		public override void Perform()
		{
			var volume = TypeLibrary.Create<IVolumeEntity>( EntityType.Identity );

			volume.Position = Mins;
			volume.SetVolume( Mins, Maxs );

			if ( EntityId > 0 )
				UpdateObject( EntityId, volume as ISourceEntity );
			else
				EntityId = AddObject( volume as ISourceEntity );

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
