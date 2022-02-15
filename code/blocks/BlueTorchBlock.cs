﻿using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class BlueTorchBlock : BaseTorchBlock
	{
		public override string FriendlyName => "Blue Torch";
		public override IntVector3 LightLevel => new IntVector3( 14, 9, 9 );
	}
}
