using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Voxel
{
	public class AirBlock : BlockType
	{
		public AirBlock( Map map )
		{
			Map = map;
		}

		public override string FriendlyName => "Air";
		public override bool IsTranslucent => true;
	}
}
