using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Utility;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class BrewItem : InventoryItem, IConsumableItem
	{
		public override bool RemoveOnDeath => true;
		public override bool CanBeDropped => true;
		public override ushort MaxStackSize => 4;
		public override Color Color => ColorPalette.Brews;
		public virtual string ConsumeSound => "brew.consume";
		public virtual string ConsumeEffect => null;
		public virtual string ActivateSound => "brew.activate";
		public virtual float ActivateDelay => 0.5f;

		public async void Consume( Player player )
		{
			StackSize--;

			if ( StackSize <= 0 )
				Remove();

			using ( Prediction.Off() )
			{
				if ( !string.IsNullOrEmpty( ConsumeSound ) )
				{
					player.PlaySound( ConsumeSound );
				}
			}

			await GameTask.DelaySeconds( ActivateDelay );

			if ( !player.IsValid() )
				return;

			if ( !string.IsNullOrEmpty( ActivateSound ) )
			{
				player.PlaySound( ActivateSound );
			}

			if ( !string.IsNullOrEmpty( ConsumeEffect ) )
			{
				var effect = Particles.Create( ConsumeEffect, player );
				effect.AutoDestroy( 3f );
				effect.SetEntity( 0, player );
			}

			if ( player.LifeState == LifeState.Alive )
			{
				OnActivated( player );
			}
		}

		public virtual void OnActivated( Player player )
		{

		}

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
