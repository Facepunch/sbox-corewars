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

	[EditorTool( Title = "Entities", Description = "Add or manipulate entities", Icon = "textures/ui/tools/entities.png" )]
	public partial class EntitiesTool : EditorTool
	{
		[ServerCmd]
		public static void ChangeLibraryAttributeCmd( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
				return;

			if ( player.Tool is not EntitiesTool tool )
				return;

			tool.SetAttribute( (EditorEntityAttribute)Library.GetAttribute( type ) );
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

		[Net, Change( nameof( OnModeChanged ) )] public EntitiesToolMode Mode { get; private set; }
		[Net, Change( nameof( OnTypeChanged ) )] public string CurrentType { get; private set; }
		[Net, Change( nameof( OnEntityChanged ) )] public ModelEntity SelectedEntity { get; private set; }

		private EditorEntityAttribute CurrentAttribute { get; set; }
		private VolumeEntity Volume { get; set; }
		private ModelEntity GhostEntity { get; set; }
		private TimeUntil NextActionTime { get; set; }
		private Vector3? StartPosition { get; set; }

		public void SetAttribute( EditorEntityAttribute attribute )
		{
			Host.AssertServer();

			if ( CurrentType != attribute.Name )
			{
				CurrentType = attribute.Name;
				CurrentAttribute = attribute;
				OnAttributeChanged( CurrentAttribute );
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

				if ( Mode == EntitiesToolMode.Place || Mode == EntitiesToolMode.MoveAndRotate )
				{
					if ( CurrentAttribute.IsVolume )
					{
						var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );
						var volumeBBox = GetVolumeBBox( StartPosition.HasValue ? StartPosition.Value : aimSourcePosition, aimSourcePosition );

						Volume.Position = volumeBBox.Mins;
						Volume.RenderBounds = new BBox( volumeBBox.Mins - Volume.Position, volumeBBox.Maxs - Volume.Position );
					}
					else if ( GhostEntity.IsValid() )
					{
						var shouldCenterOnXY = !Input.Down( InputButton.Run );
						var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, shouldCenterOnXY, shouldCenterOnXY, false );
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
				if ( string.IsNullOrEmpty( CurrentType ) )
				{
					var attribute = Library.GetAttributes<EditorEntityAttribute>().FirstOrDefault();
					SetAttribute( attribute );
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

			SelectedEntity = null;
		}

		protected virtual void OnEntityChanged( ModelEntity oldEntity, ModelEntity newEntity )
		{
			if ( oldEntity.IsValid() )
			{
				oldEntity.EnableDrawing = true;
			}

			CreateMoveGhostEntity();
		}

		protected virtual void OnTypeChanged( string type )
		{
			CurrentAttribute = (EditorEntityAttribute)Library.GetAttribute( type );
			OnAttributeChanged( CurrentAttribute );
		}

		protected virtual void OnAttributeChanged( EditorEntityAttribute attribute )
		{
			if ( IsClient )
			{
				if ( Mode == EntitiesToolMode.Place )
					CreateGhostEntity();
				else
					DestroyGhostEntity();
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			if ( TryGetTargetEntity( out var target, out var trace ) )
			{
				if ( Mode == EntitiesToolMode.MoveAndRotate && target is IVolumeEntity )
				{
					return;
				}

				var outlineColor = Color.White;

				if ( Mode == EntitiesToolMode.Remove )
					outlineColor = Color.Red;
				else if ( Mode == EntitiesToolMode.DataEditor )
					outlineColor = Color.Cyan;
				else if ( Mode == EntitiesToolMode.MoveAndRotate )
					outlineColor = Color.Yellow;

				var entityType = target.GetType();
				var worldBounds = target.WorldSpaceBounds;

				DebugOverlay.Box( worldBounds.Mins, worldBounds.Maxs, outlineColor, Time.Delta, false );

				if ( Mode == EntitiesToolMode.Remove )
					DebugOverlay.Text( trace.EndPosition, $"Delete {entityType.Name}", Color.Red, Time.Delta );
				else if ( Mode == EntitiesToolMode.DataEditor )
					DebugOverlay.Text( trace.EndPosition, $"Edit {entityType.Name}", Color.Cyan, Time.Delta );
				else if ( Mode == EntitiesToolMode.MoveAndRotate )
					DebugOverlay.Text( trace.EndPosition, $"Move {entityType.Name}", Color.Yellow, Time.Delta );
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

			if ( IsServer )
			{
				SelectedEntity = null;
			}
		}

		protected override void OnPrimary( Client client )
		{
			if ( NextActionTime )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );

				if ( Mode == EntitiesToolMode.Place )
				{
					if ( CurrentAttribute.IsVolume )
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
								action.Initialize( CurrentAttribute, bbox.Mins, bbox.Maxs );

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
						var shouldCenterOnXY = !Input.Down( InputButton.Run );
						var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, shouldCenterOnXY, shouldCenterOnXY, false );

						var action = new PlaceEntityAction();
						action.Initialize( CurrentAttribute, aimSourcePosition, Rotation.Identity );
						Player.Perform( action );
					}

					NextActionTime = 0.1f;
				}
				else if ( Mode == EntitiesToolMode.MoveAndRotate )
				{
					if ( IsServer )
					{
						if ( SelectedEntity.IsValid() )
						{
							var shouldCenterOnXY = !Input.Down( InputButton.Run );
							var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, shouldCenterOnXY, shouldCenterOnXY, false );

							var action = new MoveEntityAction();
							action.Initialize( SelectedEntity, aimSourcePosition, Rotation.Identity );
							Player.Perform( action );

							SelectedEntity = null;
						}
						else
						{
							if ( TryGetTargetEntity( out var target, out _ ) && target is not IVolumeEntity )
							{
								SelectedEntity = target as ModelEntity;
							}
						}
					}
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

		private void CreateMoveGhostEntity()
		{
			DestroyGhostEntity();

			if ( !SelectedEntity.IsValid() )
				return;

			GhostEntity = new ModelEntity( SelectedEntity.Model.Name );
			GhostEntity.RenderColor = Color.White.WithAlpha( 0.5f );
			GhostEntity.Transform = SelectedEntity.Transform;
		}

		private void CreateGhostEntity()
		{
			DestroyGhostEntity();

			if ( CurrentAttribute == null )
				return;

			if ( CurrentAttribute.IsVolume )
			{
				Volume = new VolumeEntity
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.White
				};

				if ( !string.IsNullOrEmpty( CurrentAttribute.VolumeMaterial ) )
				{
					Volume.Material = Material.Load( CurrentAttribute.VolumeMaterial );
				}
			}
			else
			{
				GhostEntity = new ModelEntity( CurrentAttribute.EditorModel );
				GhostEntity.RenderColor = Color.White.WithAlpha( 0.5f );

				if ( Mode == EntitiesToolMode.MoveAndRotate && SelectedEntity.IsValid() )
				{
					GhostEntity.Transform = SelectedEntity.Transform;
				}
			}
		}
	}
}
