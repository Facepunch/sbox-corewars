using Facepunch.CoreWars.Editor;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Text.RegularExpressions;

namespace Facepunch.CoreWars.UI
{
	public enum ChatBoxChannel
	{
		All,
		Team,
		Tip
	}

	public class TextEntryContainer : Panel
	{
		public TabTextEntry Input { get; private set; }
		public Label Channel { get; private set; }

		public TextEntryContainer()
		{
			Channel = Add.Label( "All", "channel" );
			Input = AddChild<TabTextEntry>( "" );
		}

		public void SetChannel( ChatBoxChannel channel )
		{
			Channel.SetClass( "team", channel == ChatBoxChannel.Team );
			Channel.SetClass( "all", channel == ChatBoxChannel.All );

			if ( channel == ChatBoxChannel.All )
				Channel.Text = "All";
			else
				Channel.Text = "Team";
		}
	}

	public partial class ChatBox : Panel
	{
		public static ChatBox Current { get; private set; }

		public static string[] Tips => new string[]
		{
			"This is a tip! This is just a test tip for UI styling."
		};

		public TextEntryContainer TextEntry { get; private set; }
		public Panel Canvas { get; private set; }

		private RealTimeUntil NextTipTime { get; set; }

		[ConCmd.Client( "cw_chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string avatar = null, string className = null, ChatBoxChannel channel = ChatBoxChannel.All )
		{
			Current?.AddEntry( name, message, avatar, className, channel );

			if ( !Game.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ConCmd.Client( "cw_chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message, string avatar = null )
		{
			Current?.AddEntry( null, message, avatar, "info" );
		}

		[ConCmd.Client( "cw_tip" )]
		public static void ShowTipCommand()
		{
			Current.ShowRandomTip();
		}

		[ConCmd.Server( "cw_say" )]
		public static void Say( string message, ChatBoxChannel channel )
		{
			var caller = ConsoleSystem.Caller;
			Assert.NotNull( caller );

			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $"{caller}: {message}" );

			if ( caller.Pawn is CoreWarsPlayer player )
			{
				if ( channel == ChatBoxChannel.All || player.Team == Team.None )
					AddChatEntry( To.Everyone, caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}", player.Team.GetHudClass() );
				else
					AddChatEntry( player.Team.GetTo(), caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}", player.Team.GetHudClass(), channel );
			}	
			else if ( caller.Pawn is EditorPlayer )
			{
				AddChatEntry( To.Everyone, caller.Name, message, $"avatar:{ConsoleSystem.Caller.SteamId}", Team.Orange.GetHudClass() );
			}
		}

		public ChatBoxChannel Channel { get; private set; } = ChatBoxChannel.Team;

		public ChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/chatbox/ChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			if ( CoreWarsGame.IsEditorMode )
			{
				Channel = ChatBoxChannel.All;
			}

			TextEntry = AddChild<TextEntryContainer>();
			TextEntry.Input.AddEventListener( "onsubmit", () => Submit() );
			TextEntry.Input.AddEventListener( "onblur", () => CloseOnBlur() );
			TextEntry.Input.AcceptsFocus = true;
			TextEntry.Input.AllowEmojiReplace = true;
			TextEntry.Input.OnTabPressed += OnTabPressed;
			TextEntry.SetChannel( Channel );
		}

		public void Open()
		{
			AddClass( "open" );
			TextEntry.Input.Focus();
		}

		public void Close()
		{
			RemoveClass( "open" );
			TextEntry.Input.Blur();
		}

		public void AddEntry( string name, string message, string avatar, string className = null, ChatBoxChannel channel = ChatBoxChannel.All )
		{
			var entry = Canvas.AddChild<ChatEntry>();
			entry.Message.Text = message;
			entry.NameLabel.Text = name;
			entry.Avatar.SetTexture( avatar );

			if ( !string.IsNullOrEmpty( className ) )
			{
				entry.AddClass( className );
			}

			if ( channel != ChatBoxChannel.All )
			{
				entry.SetChannel( channel );
			}

			entry.SetClass( "noname", string.IsNullOrEmpty( name ) );
			entry.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );
		}

		public override void Tick()
		{
			if ( NextTipTime && !CoreWarsGame.IsEditorMode )
			{
				ShowRandomTip();
				NextTipTime = Game.Random.Float( 45f, 60f );
			}

			if ( Input.Pressed( "chat" ) )
			{
				Open();
			}

			base.Tick();
		}

		private void ShowRandomTip()
		{
			var tip = Game.Random.FromArray( Tips );

			tip = Regex.Replace( tip, "(\\+[a-zA-Z0-9]+)", ( match ) =>
			{
				return Input.GetButtonOrigin( match.Value.Replace( "+", "" ) );
			} );

			Current?.AddEntry( null, tip, null, "tip", ChatBoxChannel.Tip );
		}

		private void OnTabPressed()
		{
			if ( CoreWarsGame.IsEditorMode )
			{
				Channel = ChatBoxChannel.All;
				TextEntry.SetChannel( Channel );
				return;
			}

			if ( Channel == ChatBoxChannel.All )
				Channel = ChatBoxChannel.Team;
			else
				Channel = ChatBoxChannel.All;

			TextEntry.SetChannel( Channel );
		}

		private void CloseOnBlur()
		{
			// Conna: Not sure why I have to do this, why does `onblur` get called when it hasn't lost focus?
			if ( InputFocus.Current != TextEntry.Input )
			{
				Close();
			}
		}

		private void Submit()
		{
			Close();

			if ( string.IsNullOrEmpty( TextEntry.Input.Text ) )
				return;

			var msg = TextEntry.Input.Text.Trim();
			TextEntry.Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			if ( TextEntry.Input.IsShiftDown )
				Say( msg, ChatBoxChannel.Team );
			else
				Say( msg, Channel );
		}
	}
}
