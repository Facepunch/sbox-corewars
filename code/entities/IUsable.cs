using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public interface IUsable
	{
		float MaxUseDistance { get; }
		bool IsUsable( CoreWarsPlayer player );
		void OnUsed( CoreWarsPlayer player );
	}
}
