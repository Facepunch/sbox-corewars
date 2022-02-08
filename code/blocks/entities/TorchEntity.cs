﻿using Facepunch.CoreWars.Voxel;
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
			var data = Map.GetData<TorchBlockData>( BlockPosition );

			if ( data.IsValid() )
			{
				CenterOnSide( Map.GetOppositeDirection( data.Direction ) );
			}

			base.Initialize();
		}
	}
}
