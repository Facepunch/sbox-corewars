using Facepunch.Voxels;
using Sandbox;
using System;

namespace Facepunch.CoreWars.Blocks
{
	[Library( "cw_torch" )]
	public class TorchEntity : BlockEntity
	{
		public override void Spawn()
		{
			SetModel( "models/citizen_props/sodacan01.vmdl" );

			base.Spawn();
		}

		public override void Initialize()
		{
			var data = VoxelWorld.GetState<TorchState>( BlockPosition );

			if ( data.IsValid() )
			{
				CenterOnSide( VoxelWorld.GetOppositeDirection( data.Direction ) );
			}

			base.Initialize();
		}
	}
}
