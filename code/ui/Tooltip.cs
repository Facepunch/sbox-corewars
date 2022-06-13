using Sandbox;
using Sandbox.UI;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Tooltip : Panel
	{
		private static Tooltip Current { get; set; }

		public static void Show( ITooltipProvider provider )
		{
			if ( Current == null || Current.Provider != provider )
			{
				var tooltip = new Tooltip();
				tooltip.SetProvider( provider );
				Hud.Current.AddChild( tooltip );
				return;
			}

			Current.TimeSinceShown = 0f;
		}

		public static void Hide( ITooltipProvider provider )
		{
			if ( Current != null && Current.Provider == provider )
			{
				Current?.Delete();
				Current = null;
			}
		}

		public ITooltipProvider Provider { get; private set; }
		public TimeSince TimeSinceShown { get; private set; }
		public Panel Container { get; private set; }
		public Label NameLabel { get; private set; }

		public void SetProvider( ITooltipProvider provider )
		{
			Provider = provider;
			NameLabel.Style.FontColor = provider.Color;
		}

		public Tooltip()
		{
			TimeSinceShown = 0f;

			Current?.Delete( true );
			Current = this;

			UpdatePosition();
		}

		public void UpdatePosition()
		{
			Container.Style.Left = Length.Pixels( (Mouse.Position.x + 40f) * ScaleFromScreen );
			Container.Style.Top = Length.Pixels( (Mouse.Position.y - 40f) * ScaleFromScreen );
		}

		protected override void FinalLayoutChildren()
		{
			Container.SetClass( "active", true );
			base.FinalLayoutChildren();
		}

		public override void Tick()
		{
			if ( Provider == null || !Provider.IsVisible )
			{
				if ( !IsDeleting )
				{
					Current = null;
					Delete();
				}
				return;
			}

			UpdatePosition();
			base.Tick();
		}
	}
}
