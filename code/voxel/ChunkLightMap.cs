using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkLightMap
	{
		public Texture Texture { get; private set; }
		public Texture Texture2 { get; private set; } // We'll nuke this later.
		public Chunk Chunk { get; private set; }
		public Map Map { get; private set; }
		public byte[] Data;
		public byte[] Data2;
		public int ChunkSize;

		public Queue<LightRemoveNode> TorchLightRemoveQueue { get; private set; } = new();
		public Queue<IntVector3> TorchLightAddQueue { get; private set; } = new();
		public Queue<LightRemoveNode> SunLightRemoveQueue { get; private set; } = new();
		public Queue<IntVector3> SunLightAddQueue { get; private set; } = new();

		public ChunkLightMap( Chunk chunk, Map map )
		{
			ChunkSize = Chunk.ChunkSize;
			Data = new byte[ChunkSize * ChunkSize * ChunkSize];
			Data2 = new byte[ChunkSize * ChunkSize * ChunkSize];
			
			Texture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( Data )
				.Finish();

			Texture2 = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.A8 )
				.WithData( Data )
				.Finish();

			Chunk = chunk;
			Map = map;
		}

		public int ToIndex( IntVector3 position, int component )
		{
			return (position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x;
		}

		public byte GetSunLight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return Data2[index];
		}

		public bool SetSunLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			if ( Data2[index] == value ) return false;
			Data2[index] = value;
			return true;
		}

		public void AddTorchLight( IntVector3 position, byte value )
		{
			if ( SetTorchLight( position, value ) )
			{
				TorchLightAddQueue.Enqueue( position );
			}
		}

		public void AddSunLight( IntVector3 position, byte value )
		{
			if ( SetSunLight( position, value ) )
			{
				SunLightAddQueue.Enqueue( position );
			}
		}

		public bool RemoveTorchLight( IntVector3 position )
		{
			TorchLightRemoveQueue.Enqueue( new LightRemoveNode
			{
				Position = position,
				Value = GetTorchLight( position )
			} );

			return SetTorchLight( position, 0 );
		}

		public void UpdateTorchLight()
		{
			while ( TorchLightRemoveQueue.Count > 0 )
			{
				var node = TorchLightRemoveQueue.Dequeue();

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( node.Position, i );
					var neighbourBlockInfo = Map.GetBlockInfo( neighbourPosition );
					if ( !neighbourBlockInfo.IsValid ) continue;

					var lightLevel = Map.GetTorchlight( neighbourBlockInfo.Position );

					if ( lightLevel != 0 && lightLevel < node.Value )
					{
						Map.SetTorchlight( neighbourBlockInfo.Position, 0 );

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
				var lightLevel = Map.GetTorchlight( nodePosition );

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( nodePosition, i );
					var neighbourBlockInfo = Map.GetBlockInfo( neighbourPosition );
					if ( !neighbourBlockInfo.IsValid ) continue;

					var neighbourBlock = Map.GetBlockType( neighbourBlockInfo.BlockId );

					if ( Map.GetTorchlight( neighbourBlockInfo.Position ) + 2 <= lightLevel )
					{
						if ( neighbourBlock.IsTranslucent )
						{
							Map.SetTorchlight( neighbourBlockInfo.Position, (byte)(lightLevel - 1) );
							TorchLightAddQueue.Enqueue( neighbourPosition );
						}
					}
				}
			}

			Texture.Update( Data );
		}

		public bool RemoveSunLight( IntVector3 position )
		{
			SunLightRemoveQueue.Enqueue( new LightRemoveNode
			{
				Position = position,
				Value = GetSunLight( position )
			} );

			return SetSunLight( position, 0 );
		}

		public byte GetTorchLight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			return Data[index];
		}

		public bool SetTorchLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			if ( Data[index] == value ) return false;
			Data[index] = value;
			return true;
		}

		public void Update()
		{
			Texture.Update( Data );
			Texture2.Update( Data2 );
		}
	}
}
