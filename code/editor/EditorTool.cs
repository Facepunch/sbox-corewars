using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public partial class EditorTool : BaseNetworkable, IValid
	{
		public virtual string Name => "Tool";

		[Net] public EditorPlayer Player { get; set; }

		public bool IsClient => Host.IsClient;
		public bool IsServer => Host.IsServer;
		public bool IsValid => true;

		public virtual void Simulate( Client client )
		{
			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				OnPrimary( client );
			}

			if ( Input.Pressed( InputButton.Attack2 ) )
			{
				OnSecondary( client );
			}
		}

		public virtual void OnSelected()
		{

		}

		public virtual void OnDeselected()
		{

		}

		protected virtual void OnPrimary( Client client )
		{

		}

		protected virtual void OnSecondary( Client client )
		{

		}
	}
}
