﻿using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class VortexBombBlock : BaseBuildingBlock
	{
		public override string Icon => "textures/items/vortex_bomb.png";
		public override string FriendlyName => "Vortex Bomb";
		public override string Description => "A high damage timed explosive bomb which can only be neutralized with a Watergun.";
		public override string ServerEntity => "cw_vortex_bomb";
		public override bool IsTranslucent => true;
		public override bool HasTexture => false;
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Explosives;

		public override bool ShouldCullFace( BlockFace face, BlockType neighbour )
		{
			return false;
		}
	}
}