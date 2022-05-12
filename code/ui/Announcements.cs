using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public partial class Announcements : Panel
	{
		public static Announcements Instance { get; private set; }
		public Queue<Announcement> Queue { get; private set; }
		public Panel Container { get; set; }

		[ClientRpc]
		public static void Send( string title, string text, string icon )
		{
			var announcement = new Announcement();
			announcement.Update( title, text );
			announcement.SetIcon( icon );
			Instance.AddItem( announcement );
		}

		public Announcements()
		{
			StyleSheet.Load( "/ui/Announcements.scss" );
			Container = Add.Panel( "container" );
			Instance = this;
			Queue = new();
		}

		public void AddItem( Announcement item )
		{
			Queue.Enqueue( item );
		}

		public void Next()
		{
			if ( Queue.Count > 0 )
			{
				Audio.Play( "ui.announcement" );
				var item = Queue.Dequeue();
				item.EndTime = Time.Now + 5f;
				Container.AddChild( item );
			}
		}

		public override void Tick()
		{
			if ( Queue.Count > 0 && Container.ChildrenCount == 0 )
			{
				Next();
			}

			base.Tick();
		}
	}
}
