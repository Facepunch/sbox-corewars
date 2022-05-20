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
		private HashSet<Player> Touching { get; set; } = new();

		public override void StartTouch( Entity other )
		{
			if ( other is Player player )
				Touching.Add( player );

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( other is Player player )
				Touching.Remove( player );

			base.EndTouch( other );
		}
	}
}
