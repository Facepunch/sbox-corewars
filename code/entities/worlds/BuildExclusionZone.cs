using Facepunch.CoreWars.Editor;
using Sandbox;
using System.Collections.Generic;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Build Exclusion Zone", IsVolume = true, VolumeMaterial = "materials/editor/build_exclusion.vmat" )]
	[Category( "Triggers" )]
	public partial class BuildExclusionZone : BaseTrigger
	{
		private HashSet<CoreWarsPlayer> Touching { get; set; } = new();

		public override void StartTouch( Entity other )
		{
			if ( other is CoreWarsPlayer player )
				Touching.Add( player );

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( other is CoreWarsPlayer player )
				Touching.Remove( player );

			base.EndTouch( other );
		}
	}
}
