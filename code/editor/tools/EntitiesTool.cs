using Facepunch.Voxels;
using Sandbox;
using System;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	public enum EntitiesToolMode
	{
		Place,
		Remove,
		MoveAndRotate,
		DataEditor
	}

	[EditorToolLibrary( Title = "Entities", Description = "Add or manipulate entities", Icon = "textures/ui/tools/entities.png" )]
	public partial class EntitiesTool : EditorTool
	{
		[ServerCmd]
		public static void ChangeLibraryAttributeCmd( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
				return;

			if ( player.Tool is not EntitiesTool tool )
				return;

			tool.SetLibraryAttribute( (EditorEntityLibraryAttribute)Library.GetAttribute( type ) );
		}

		[ServerCmd]
		public static void ChangeModeCmd( int mode )
		{
			if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
				return;

			if ( player.Tool is not EntitiesTool tool )
				return;
			
			tool.SetMode( (EntitiesToolMode)mode );
		}

		[Net, Change( nameof( OnModeChanged ))] public EntitiesToolMode Mode { get; private set; }
		[Net, Change( nameof( OnLibraryTypeChanged ))] public string CurrentLibraryType { get; private set; }

		private EditorEntityLibraryAttribute CurrentLibraryAttribute { get; set; }
		private VolumeEntity Volume { get; set; }
		private ModelEntity GhostEntity { get; set; }
		private TimeUntil NextActionTime { get; set; }
		private Vector3? StartPosition { get; set; }

		public void SetLibraryAttribute( EditorEntityLibraryAttribute attribute )
		{
			Host.AssertServer();

			if ( CurrentLibraryType != attribute.Name )
			{
				CurrentLibraryType = attribute.Name;
				CurrentLibraryAttribute = attribute;
				OnLibraryAttributeChanged( CurrentLibraryAttribute );
			}
		}

		public void SetMode( EntitiesToolMode mode )
		{
			Host.AssertServer();

			if ( Mode != mode )
			{
				Mode = mode;
				OnModeChanged( mode );
			}
		}

		public override void Simulate( Client client )
		{
			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );

				if ( Mode == EntitiesToolMode.Place )
				{
					if ( CurrentLibraryAttribute.IsVolume )
					{
						var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );
						var volumeBBox = GetVolumeBBox( StartPosition.HasValue ? StartPosition.Value : aimSourcePosition, aimSourcePosition );

						Volume.Position = volumeBBox.Mins;
						Volume.RenderBounds = new BBox( volumeBBox.Mins - Volume.Position, volumeBBox.Maxs - Volume.Position );
					}
					else if ( GhostEntity.IsValid() )
					{
						var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, true, true, false );
						GhostEntity.Position = aimSourcePosition;
					}
				}
			}

			base.Simulate( client );
		}

		public override void OnSelected()
		{
			if ( IsServer )
			{
				if ( string.IsNullOrEmpty( CurrentLibraryType ) )
				{
					var attribute = Library.GetAttributes<EditorEntityLibraryAttribute>().FirstOrDefault();
					SetLibraryAttribute( attribute );
				}

				SetMode( EntitiesToolMode.Place );
			}

			if ( IsClient )
			{
				OnModeChanged( Mode );
			}

			Event.Register( this );

			NextActionTime = 0.1f;
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				DestroyGhostEntity();
			}

			Event.Unregister( this );
		}

		protected virtual void OnLibraryTypeChanged( string type )
		{
			CurrentLibraryAttribute = (EditorEntityLibraryAttribute)Library.GetAttribute( type );
			OnLibraryAttributeChanged( CurrentLibraryAttribute );
		}

		protected virtual void OnLibraryAttributeChanged( EditorEntityLibraryAttribute attribute )
		{
			if ( IsClient )
			{
				if ( Mode == EntitiesToolMode.Place )
				{
					CreateGhostEntity();
				}
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( TryGetTargetEntity( out var target, out var trace ) )
			{
				var outlineColor = Color.White;

				if ( Mode == EntitiesToolMode.Remove )
					outlineColor = Color.Red;
				else if ( Mode == EntitiesToolMode.DataEditor )
					outlineColor = Color.Cyan;

				var entityType = target.GetType();
				var worldBounds = target.WorldSpaceBounds;

				DebugOverlay.Box( worldBounds.Mins, worldBounds.Maxs, outlineColor, Time.Delta, false );

				if ( Mode == EntitiesToolMode.Remove )
					DebugOverlay.Text( trace.EndPosition, $"Delete {entityType.Name}", Color.Red, Time.Delta );
				else if ( Mode == EntitiesToolMode.DataEditor )
					DebugOverlay.Text( trace.EndPosition, $"Edit {entityType.Name}", Color.Cyan, Time.Delta );
				else
					DebugOverlay.Text( trace.EndPosition, entityType.Name, Time.Delta );
			}
		}

		protected virtual void OnModeChanged( EntitiesToolMode mode )
		{
			if ( IsClient )
			{
				if ( Mode == EntitiesToolMode.Place )
					CreateGhostEntity();
				else
					DestroyGhostEntity();
			}
		}

		protected override void OnPrimary( Client client )
		{
			if ( NextActionTime )
			{
				if ( Mode == EntitiesToolMode.Place )
				{
					var aimVoxelPosition = GetAimVoxelPosition( 4f );

					if ( CurrentLibraryAttribute.IsVolume )
					{
						var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

						if ( StartPosition.HasValue )
						{
							if ( IsServer )
							{
								var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
								var endVoxelPosition = aimVoxelPosition;

								var bbox = GetVolumeBBox(
									VoxelWorld.Current.ToSourcePosition( startVoxelPosition ),
									VoxelWorld.Current.ToSourcePosition( endVoxelPosition )
								);

								var action = new PlaceVolumeAction();
								action.Initialize( CurrentLibraryAttribute, bbox.Mins, bbox.Maxs );

								Player.Perform( action );
							}

							CreateGhostEntity();

							StartPosition = null;
						}
						else
						{
							StartPosition = aimSourcePosition;
						}
					}
					else if ( IsServer )
					{
						var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, true, true, false );

						var action = new PlaceEntityAction();
						action.Initialize( CurrentLibraryAttribute, aimSourcePosition, Rotation.Identity );
						Player.Perform( action );
					}

					NextActionTime = 0.1f;
				}
				else if ( Mode == EntitiesToolMode.Remove )
				{
					if ( IsServer )
					{
						if ( TryGetTargetEntity( out var target, out _ ) )
						{
							var action = new RemoveEntityAction();
							action.Initialize( target );
							Player.Perform( action );
						}
					}
				}
				else if ( Mode == EntitiesToolMode.DataEditor )
				{
					if ( IsClient )
					{
						if ( TryGetTargetEntity( out var target, out _ ) )
						{
							EditorEntityData.SendOpenRequest( target.NetworkIdent );
						}
					}
				}
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( IsServer )
			{
				
			}
		}

		private bool TryGetTargetEntity( out ISourceEntity target, out TraceResult trace )
		{
			 trace = Trace.Ray( Input.Position, Input.Position + Input.Rotation.Forward * 5000f )
				.EntitiesOnly()
				.Run();

			if ( trace.Entity.IsValid() && trace.Entity is ISourceEntity )
			{
				target = trace.Entity as ISourceEntity;
				return true;
			}

			target = default;
			return false;
		}

		private BBox GetVolumeBBox( Vector3 startPosition, Vector3 endPosition )
		{
			var startBlock = new BBox( startPosition, startPosition + new Vector3( VoxelWorld.Current.VoxelSize ) );
			var endBlock = new BBox( endPosition, endPosition + new Vector3( VoxelWorld.Current.VoxelSize ) );
			var worldBBox = new BBox( startBlock.Mins, startBlock.Maxs );

			worldBBox = worldBBox.AddPoint( endBlock.Mins );
			worldBBox = worldBBox.AddPoint( endBlock.Maxs );

			return worldBBox;
		}

		private void DestroyGhostEntity()
		{
			Volume?.Delete();
			Volume = null;

			GhostEntity?.Delete();
			GhostEntity = null;
		}

		private void CreateGhostEntity()
		{
			DestroyGhostEntity();

			if ( CurrentLibraryAttribute == null )
				return;

			if ( CurrentLibraryAttribute.IsVolume )
			{
				Volume = new VolumeEntity
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.White
				};

				if ( !string.IsNullOrEmpty( CurrentLibraryAttribute.VolumeMaterial ) )
				{
					Volume.Material = Material.Load( CurrentLibraryAttribute.VolumeMaterial );
				}
			}
			else
			{
				GhostEntity = new ModelEntity( CurrentLibraryAttribute.EditorModel );
				GhostEntity.RenderColor = Color.White.WithAlpha( 0.5f );
			}
		}
	}
}
