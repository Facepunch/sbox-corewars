﻿using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WindowBlock : BlockType
	{
		public override string DefaultTexture => "window";
		public override string FriendlyName => "Window";
		public override bool IsTranslucent => false;
		public override IntVector3 LightLevel => new IntVector3( 14, 0, 0 );
	}
}
