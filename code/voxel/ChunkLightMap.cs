using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkLightMap
	{
		public Texture TorchLightTexture { get; private set; }
		public Texture SunLightTexture { get; private set; } // We'll nuke this later.
		public Chunk Chunk { get; private set; }
		public Map Map { get; private set; }
		public byte[] TorchLightData;
		public byte[] SunLightData;
		public int ChunkSize;

		public Queue<LightRemoveNode> TorchLightRemoveQueue { get; private set; } = new();
		public Queue<IntVector3> TorchLightAddQueue { get; private set; } = new();
		public Queue<LightRemoveNode> SunLightRemoveQueue { get; private set; } = new();
		public Queue<IntVector3> SunLightAddQueue { get; private set; } = new();

		private bool IsTorchLightDirty { get; set; }
		private bool IsSunLightDirty { get; set; }

		public ChunkLightMap( Chunk chunk, Map map )
		{
			ChunkSize = Chunk.ChunkSize;
			Chunk = chunk;
			Map = map;

			TorchLightData = new byte[ChunkSize * ChunkSize * ChunkSize];
			TorchLightTexture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( TorchLightData )
				.Finish();

			SunLightData = new byte[ChunkSize * ChunkSize * ChunkSize];
			SunLightTexture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( SunLightData )
				.Finish();
		}

		public int ToIndex( IntVector3 position, int component )
		{
			return (position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x;
		}

		public byte GetSunLight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return SunLightData[index];
		}

		public bool SetSunLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			if ( SunLightData[index] == value ) return false;
			IsSunLightDirty = true;
			SunLightData[index] = value;
			return true;
		}

		public void AddTorchLight( IntVector3 position, byte value )
		{
			if ( SetTorchLight( position, value ) )
			{
				TorchLightAddQueue.Enqueue( Chunk.Offset + position );
			}
		}

		public void AddSunLight( IntVector3 position, byte value )
		{
			if ( SetSunLight( position, value ) )
			{
				SunLightAddQueue.Enqueue( Chunk.Offset + position );
			}
		}

		public bool RemoveTorchLight( IntVector3 position )
		{
			TorchLightRemoveQueue.Enqueue( new LightRemoveNode
			{
				Position = Chunk.Offset + position,
				Value = GetTorchLight( position )
			} );

			return SetTorchLight( position, 0 );
		}

		public void UpdateSunLight()
		{
			while ( SunLightRemoveQueue.Count > 0 )
			{
				var node = SunLightRemoveQueue.Dequeue();

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( node.Position, i );
					var lightLevel = Map.GetSunLight( neighbourPosition );

					if ( (lightLevel == 15 && neighbourPosition.z == node.Position.z - 1) || (lightLevel != 0 && lightLevel < node.Value) )
					{
						Map.SetSunLight( neighbourPosition, 0 );

						SunLightRemoveQueue.Enqueue( new LightRemoveNode
						{
							Position = neighbourPosition,
							Value = lightLevel
						} );
					}
					else if ( lightLevel >= node.Value )
					{
						SunLightAddQueue.Enqueue( neighbourPosition );
					}
				}
			}

			while ( SunLightAddQueue.Count > 0 )
			{
				var node = SunLightAddQueue.Dequeue();
				var blockId = Map.GetBlock( node );
				var block = Map.GetBlockType( blockId );

				if ( !block.IsTranslucent )
					continue;

				var lightLevel = Map.GetSunLight( node );

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( node, i );
					var neighbourLightLevel = Map.GetSunLight( neighbourPosition );

					if ( neighbourLightLevel + 2 <= lightLevel || (lightLevel == 15 && neighbourLightLevel != 15 && neighbourPosition.z == node.z - 1) )
					{
						var neighbourBlockId = Map.GetBlock( neighbourPosition );
						var neighbourBlock = Map.GetBlockType( neighbourBlockId );

						if ( neighbourBlock.IsTranslucent )
						{
							if ( lightLevel == 15 && neighbourPosition.z == node.z - 1 && !neighbourBlock.AttenuatesSunLight )
							{
								Map.AddSunLight( neighbourPosition, lightLevel );
							}
							else if ( lightLevel == 15 && neighbourPosition.z == node.z + 1 )
							{
								continue;
							}
							else
							{
								Map.AddSunLight( neighbourPosition, (byte)(lightLevel - 1) );
							}
						}
					}
				}
			}

			if ( IsSunLightDirty )
			{
				IsSunLightDirty = false;
				SunLightTexture.Update( SunLightData );
			}
		}

		public void UpdateTorchLight()
		{
			while ( TorchLightRemoveQueue.Count > 0 )
			{
				var node = TorchLightRemoveQueue.Dequeue();

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( node.Position, i );
					var lightLevel = Map.GetTorchLight( neighbourPosition );

					if ( lightLevel != 0 && lightLevel < node.Value )
					{
						Map.SetTorchLight( neighbourPosition, 0 );

						TorchLightRemoveQueue.Enqueue( new LightRemoveNode
						{
							Position = neighbourPosition,
							Value = node.Value
						} );
					}
					else if ( lightLevel >= node.Value )
					{
						TorchLightAddQueue.Enqueue( neighbourPosition );
					}
				}
			}

			while ( TorchLightAddQueue.Count > 0 )
			{
				var nodePosition = TorchLightAddQueue.Dequeue();
				var lightLevel = Map.GetTorchLight( nodePosition );

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( nodePosition, i );
					var neighbourBlockInfo = Map.GetBlockInfo( neighbourPosition );
					if ( !neighbourBlockInfo.IsValid ) continue;

					var neighbourBlock = Map.GetBlockType( neighbourBlockInfo.BlockId );

					if ( Map.GetTorchLight( neighbourBlockInfo.Position ) + 2 <= lightLevel )
					{
						if ( neighbourBlock.IsTranslucent )
						{
							Map.AddTorchLight( neighbourPosition, (byte)(lightLevel - 1) );
						}
					}
				}
			}

			if ( IsTorchLightDirty )
			{
				IsTorchLightDirty = false;
				TorchLightTexture.Update( TorchLightData );
			}
		}

		public bool RemoveSunLight( IntVector3 position )
		{
			SunLightRemoveQueue.Enqueue( new LightRemoveNode
			{
				Position = Chunk.Offset + position,
				Value = GetSunLight( position )
			} );

			return SetSunLight( position, 0 );
		}

		public byte GetTorchLight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return TorchLightData[index];
		}

		public bool SetTorchLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			if ( TorchLightData[index] == value ) return false;
			IsTorchLightDirty = true;
			TorchLightData[index] = value;
			return true;
		}
	}
}
