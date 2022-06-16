using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	public partial class SpeedBuff : BaseBuff
	{
		public override string Icon => "textures/items/brew_speed.png";

		public override void OnActivated( Player player )
		{
			base.OnActivated( player );
		}

		public override void OnExpired( Player player )
		{
			base.OnExpired( player );
		}
	}
}
