using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class Tooltip : Panel
	{
		private static Tooltip Current { get; set; }

		public static Tooltip Show( ITooltipProvider provider )
		{
			if ( Current == null || Current.Provider != provider )
			{
				var tooltip = new Tooltip();
				tooltip.SetProvider( provider );
				Game.Hud.AddChild( tooltip );
				return tooltip;
			}

			Current.TimeSinceShown = 0f;
			return Current;
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
		public Panel TagContainer { get; private set; }
		public Panel Container { get; private set; }
		public Label DescriptionLabel { get; private set; }
		public Label NameLabel { get; private set; }

		public void SetProvider( ITooltipProvider provider )
		{
			Provider = provider;
			NameLabel.Style.TextShadow = new ShadowList();
			NameLabel.Style.TextShadow.Add( new Shadow()
			{
				Color = Color.Lerp( provider.Color, Color.Black, 0.3f ).WithAlpha( 0.3f ),
				OffsetX = 0f,
				OffsetY = 0f,
				Spread = -3f,
				Blur = 8f
			} );
			NameLabel.Style.FontColor = provider.Color;

			DescriptionLabel.SetClass( "hidden", string.IsNullOrEmpty( provider.Description ) );

			if ( provider.Tags != null && provider.Tags.Length > 0 )
			{
				foreach ( var tag in provider.Tags )
				{
					var panel = TagContainer.Add.Panel( "tag" );
					var label = panel.Add.Label( tag.Name, "name" );
					panel.Style.BackgroundColor = Color.Lerp( tag.Color, Color.Black, 0.7f );
					label.Style.FontColor = Color.Lerp( tag.Color, Color.Black, 0.3f );
				}
			}
			else
			{
				TagContainer.Delete();
			}
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
			Container.Style.Top = Length.Pixels( (Mouse.Position.y - 100f) * ScaleFromScreen );
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
