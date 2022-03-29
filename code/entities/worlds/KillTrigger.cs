using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntityLibrary( IsVolume = true, VolumeMaterial = "materials/tools/toolstrigger.vmat" )]
	public partial class KillTrigger : BaseTrigger
	{
		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
		}
	}
}
