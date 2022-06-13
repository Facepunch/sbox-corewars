using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Airdrop Spawnpoint", EditorModel = "models/rust_props/wooden_crates/wooden_crate_d.vmdl" )]
	[Category( "Gameplay" )]
	public partial class AirdropSpawnpoint : ModelEntity, ISourceEntity
	{
		public override void Spawn()
		{
			SetModel( "models/rust_props/wooden_crates/wooden_crate_d.vmdl" );

			var isEditorMode = Game.Current.IsEditorMode;

			EnableDrawing = isEditorMode;
			Transmit = isEditorMode ? TransmitType.Always : TransmitType.Never;

			if ( isEditorMode )
			{
				SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
				EnableSolidCollisions = false;
			}

			base.Spawn();
		}

		public virtual void Deserialize( BinaryReader reader )
		{

		}

		public virtual void Serialize( BinaryWriter writer )
		{

		}
	}
}
