using Sandbox;

namespace Facepunch.CoreWars
{
	[Library( "cw_player_spawnpoint" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Player Spawnpoint", "Core Wars", "Defines a point where players can spawn" )]
	public partial class PlayerSpawnpoint : Entity
	{
		[Property] public Team Team { get; set; }
	}
}
