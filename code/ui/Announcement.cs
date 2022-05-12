using Sandbox.UI;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public class Announcement : Panel
	{
		public float EndTime { get; set; }
		public Label Reward { get; set; }
		public Label Title { get; set; }
		public Label Text { get; set; }
		public Image Icon { get; set; }

		public void Update( string title, string text )
		{
			Title.Text = title;
			Text.Text = text;
		}

		public void SetIcon( string icon )
		{
			Icon.SetTexture( icon );
		}

		public void SetIcon( Texture texture )
		{
			Icon.Texture = texture;
		}

		public override void Tick()
		{
			if ( !IsDeleting && Time.Now >= EndTime )
			{
				Delete();
			}
		}
	}
}
