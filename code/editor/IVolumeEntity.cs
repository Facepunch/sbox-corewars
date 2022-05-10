using System.IO;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public interface IVolumeEntity : IValid
	{
		Vector3 Position { get; set; }
		Rotation Rotation { get; set; }
		Vector3 Mins { get; set; }
		Vector3 Maxs { get; set; }
		void SetVolume( Vector3 mins, Vector3 maxs );
	}
}

