﻿using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GreenTorchBlock : BlockType
	{
		public override string FriendlyName => "Green Torch";
		public override bool IsTranslucent => true;
		public override string ClientEntity => "cw_torch";
		public override bool HasTexture => false;
		public override bool IsPassable => true;
		public override IntVector3 LightLevel => new IntVector3( 0, 6, 0 );
	}
}
