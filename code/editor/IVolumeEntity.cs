using System.IO;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public interface IVolumeEntity
	{
		Vector3 Position { get; set; }
		Rotation Rotation { get; set; }
		void SetVolume( Vector3 mins, Vector3 maxs );
	}
}

