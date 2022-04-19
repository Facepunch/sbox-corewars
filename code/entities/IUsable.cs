using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public interface IUsable
	{
		float MaxUseDistance { get; }
		bool IsUsable( Player player );
		void OnUsed( Player player );
	}
}
