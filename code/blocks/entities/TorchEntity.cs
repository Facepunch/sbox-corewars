using Facepunch.CoreWars.Voxel;
using Sandbox;

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
	}
}
