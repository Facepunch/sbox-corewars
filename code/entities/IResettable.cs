using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public interface IResettable
	{
		public static void ResetAll()
		{
			foreach ( var entity in Entity.All.OfType<IResettable>() )
			{
				entity.Reset();
			}
		}

		void Reset();
	}
}
