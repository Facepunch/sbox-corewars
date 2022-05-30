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

	[EditorTool( Title = "Entities", Description = "Add or manipulate entities" )]
	[Icon( "textures/ui/tools/entities.png" )]
	public partial class EntitiesTool : EditorTool
	{
		[ConCmd.Server]
		public static void ChangeEntityToolCmd( string typeName )
		{
			if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
				return;

			if ( player.Tool is not EntitiesTool tool )
				return;

			var type = TypeLibrary.GetTypeByName( typeName );
			tool.SetTypeDescription( TypeLibrary.GetDescription( type ) );
		}

		[ConCmd.Server]
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
		[Net] public Rotation CurrentRotation { get; private set; }

		public override string SecondaryMode => Mode.ToString().ToTitleCase();

		private TypeDescription CurrentTypeDescription { get; set; }
		private EditorEntityAttribute CurrentAttribute { get; set; }
		private VolumeEntity Volume { get; set; }
		private ModelEntity GhostEntity { get; set; }
		private TimeUntil NextActionTime { get; set; }
		private Vector3? StartPosition { get; set; }

		public void SetTypeDescription( TypeDescription description )
		{
			Host.AssertServer();

			if ( CurrentType != description.ClassName )
			{
				CurrentType = description.ClassName;
				CurrentAttribute = description.GetAttribute<EditorEntityAttribute>();
				CurrentTypeDescription = description;
				OnTypeDescriptionChanged( description );
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

			if ( IsClient && currentMap.IsValid() && CurrentAttribute != null )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );

				if ( Mode == EntitiesToolMode.Place || Mode == EntitiesToolMode.MoveAndRotate )
				{
					if ( Mode == EntitiesToolMode.Place && CurrentAttribute.IsVolume )
					{
						if ( Volume.IsValid() )
						{
							var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );
							var volumeBBox = GetVolumeBBox( StartPosition.HasValue ? StartPosition.Value : aimSourcePosition, aimSourcePosition );

							Volume.Position = volumeBBox.Mins;
							Volume.RenderBounds = new BBox( volumeBBox.Mins - Volume.Position, volumeBBox.Maxs - Volume.Position );
						}
					}
					else if ( GhostEntity.IsValid() )
					{
						var shouldCenterOnXY = !Input.Down( InputButton.Run );
						var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, shouldCenterOnXY, shouldCenterOnXY, false );
						GhostEntity.Position = aimSourcePosition;
						GhostEntity.Rotation = CurrentRotation;
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
					var description = TypeLibrary.GetDescriptions<Entity>().Where( d => d.GetAttribute<EditorEntityAttribute>() != null ).FirstOrDefault();
					SetTypeDescription( description );
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

			if ( IsServer && SelectedEntity.IsValid() )
			{
				SelectedEntity.EnableDrawing = true;
				SelectedEntity = null;
			}
		}

		protected virtual void OnEntityChanged( ModelEntity oldEntity, ModelEntity newEntity )
		{
			CreateMoveGhostEntity();
		}

		protected virtual void OnTypeChanged( string typeName )
		{
			var type = TypeLibrary.GetTypeByName( typeName );
			CurrentAttribute = TypeLibrary.GetAttribute<EditorEntityAttribute>( type );
			CurrentTypeDescription = TypeLibrary.GetDescription( type );
			OnTypeDescriptionChanged( CurrentTypeDescription );
		}

		protected virtual void OnTypeDescriptionChanged( TypeDescription attribute )
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
					DebugOverlay.Text( $"Delete {entityType.Name}", trace.EndPosition, Color.Red, Time.Delta );
				else if ( Mode == EntitiesToolMode.DataEditor )
					DebugOverlay.Text( $"Edit {entityType.Name}", trace.EndPosition, Color.Cyan, Time.Delta );
				else if ( Mode == EntitiesToolMode.MoveAndRotate )
					DebugOverlay.Text( $"Move {entityType.Name}", trace.EndPosition, Color.Yellow, Time.Delta );
				else
					DebugOverlay.Text( entityType.Name, trace.EndPosition, Time.Delta );
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

			if ( IsServer && SelectedEntity.IsValid() )
			{
				SelectedEntity.EnableDrawing = true;
				SelectedEntity = null;
			}

			StartPosition = null;
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
								action.Initialize( CurrentTypeDescription, bbox.Mins, bbox.Maxs );

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
						action.Initialize( CurrentTypeDescription, aimSourcePosition, CurrentRotation );
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
							action.Initialize( SelectedEntity, aimSourcePosition, CurrentRotation );
							Player.Perform( action );

							SelectedEntity.EnableDrawing = true;
							SelectedEntity = null;
						}
						else
						{
							if ( TryGetTargetEntity( out var target, out _ ) && target is not IVolumeEntity )
							{
								SelectedEntity = target as ModelEntity;
								SelectedEntity.EnableDrawing = false;
								CurrentRotation = SelectedEntity.Rotation;
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
				CurrentRotation = CurrentRotation.RotateAroundAxis( Vector3.Up, 90f );
			}
		}

		private bool TryGetTargetEntity( out ISourceEntity target, out TraceResult trace )
		{
			var request = Trace.Ray( Input.Position, Input.Position + Input.Rotation.Forward * 5000f )
				.EntitiesOnly();

			if ( Input.Down( InputButton.Run ) )
				request = request.WithoutTags( "volume" );

			trace = request.Run();

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
			}
		}
	}
}
