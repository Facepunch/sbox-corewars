using Facepunch.CoreWars.Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;

namespace Facepunch.CoreWars
{
	[EditorEntity( Title = "Trigger Hurt", IsVolume = true, VolumeMaterial = "materials/editor/trigger_hurt.vmat" )]
	[ClassName( "cw_trigger_hurt" )]
	[Category( "Triggers" )]
	public partial class TriggerHurt : BaseTrigger
	{
		[EditorProperty, Range( 1f, 100f, 1f )]
		public float DamagePerSecond { get; set; }

		private HashSet<CoreWarsPlayer> Touching { get; set; } = new();
		private TimeSince LastDamageTime { get; set; }

		public override void StartTouch( Entity other )
		{
			if ( other is CoreWarsPlayer player )
				Touching.Add( player );

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( other is CoreWarsPlayer player )
				Touching.Remove( player );

			base.EndTouch( other );
		}

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( DamagePerSecond );

			base.Serialize( writer );
		}

		public override void Deserialize( BinaryReader reader )
		{
			DamagePerSecond = reader.ReadSingle();

			base.Deserialize( reader );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( LastDamageTime > 1f )
			{
				foreach ( var player in Touching )
				{
					if ( player.IsValid() )
					{
						var damage = new DamageInfo
						{
							Damage = DamagePerSecond,
							Position = player.Position,
							Attacker = this,
							Weapon = this
						};

						player.TakeDamage( damage );
					}
				}

				LastDamageTime = 0f;
			}
		}
	}
}
