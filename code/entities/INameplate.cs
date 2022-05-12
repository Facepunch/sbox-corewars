using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public interface INameplate : IValid
	{
		public BBox WorldSpaceBounds { get; }
		public string DisplayName { get; }
		public bool IsFriendly { get; }
		public LifeState LifeState { get; }
		public Vector3 Position { get; }
		public Team Team { get; }
	}
}
