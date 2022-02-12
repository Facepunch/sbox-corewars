using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class ChunkLightMap
	{
		public Texture Texture { get; private set; }
		public Chunk Chunk { get; private set; }
		public Map Map { get; private set; }
		public byte[] Data;
		public int ChunkSize;

		public Queue<LightRemoveNode>[] TorchLightRemoveQueue { get; private set; }
		public Queue<LightAddNode>[] TorchLightAddQueue { get; private set; }
		public Queue<LightRemoveNode> SunLightRemoveQueue { get; private set; } = new();
		public Queue<IntVector3> SunLightAddQueue { get; private set; } = new();

		private bool IsDirty { get; set; }

		public ChunkLightMap( Chunk chunk, Map map )
		{
			TorchLightRemoveQueue = new Queue<LightRemoveNode>[3];
			TorchLightAddQueue = new Queue<LightAddNode>[3];

			for ( var i = 0; i < 3; i++ )
				TorchLightRemoveQueue[i] = new();

			for ( var i = 0; i < 3; i++ )
				TorchLightAddQueue[i] = new();

			ChunkSize = Chunk.ChunkSize;
			Chunk = chunk;
			Map = map;

			Data = new byte[ChunkSize * ChunkSize * ChunkSize * 4];
			Texture = Texture.CreateVolume( ChunkSize, ChunkSize, ChunkSize )
				.WithFormat( ImageFormat.R32F )
				.WithData( Data )
				.Finish();
		}

		public int ToIndex( IntVector3 position, int component )
		{
			return (((position.z * ChunkSize * ChunkSize) + (position.y * ChunkSize) + position.x) * 4) + component;
		}

		public bool IsInBounds( int index )
		{
			return (index >= 0 && index < Data.Length);
		}

		public byte GetSunLight( IntVector3 position )
		{
			var index = ToIndex( position, 1 );
			if ( !IsInBounds( index ) ) return 0;
			return (byte)((Data[index] >> 4) & 0xF);
		}

		public bool SetSunLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 1 );
			if ( !IsInBounds( index ) ) return false;
			if ( GetSunLight( position ) == value ) return false;
			IsDirty = true;
			Data[index] = (byte)((Data[index] & 0x0F) | ((value & 0xf) << 4));
			Data[ToIndex( position, 3 )] |= 0x40;
			return true;
		}

		public void AddRedTorchLight( IntVector3 position, byte value )
		{
			if ( SetRedTorchLight( position, value ) )
			{
				TorchLightAddQueue[0].Enqueue( new LightAddNode
				{
					Position = Chunk.Offset + position,
					Channel = 0
				} );
			}
		}

		public void AddGreenTorchLight( IntVector3 position, byte value )
		{
			if ( SetGreenTorchLight( position, value ) )
			{
				TorchLightAddQueue[1].Enqueue( new LightAddNode
				{
					Position = Chunk.Offset + position,
					Channel = 1
				} );
			}
		}

		public void AddBlueTorchLight( IntVector3 position, byte value )
		{
			if ( SetBlueTorchLight( position, value ) )
			{
				TorchLightAddQueue[2].Enqueue( new LightAddNode
				{
					Position = Chunk.Offset + position,
					Channel = 2
				} );
			}
		}

		public void AddSunLight( IntVector3 position, byte value )
		{
			if ( SetSunLight( position, value ) )
			{
				SunLightAddQueue.Enqueue( Chunk.Offset + position );
			}
		}

		public bool RemoveRedTorchLight( IntVector3 position )
		{
			TorchLightRemoveQueue[0].Enqueue( new LightRemoveNode
			{
				Position = Chunk.Offset + position,
				Channel = 0,
				Value = GetRedTorchLight( position )
			} );

			return SetRedTorchLight( position, 0 );
		}

		public bool RemoveGreenTorchLight( IntVector3 position )
		{
			TorchLightRemoveQueue[1].Enqueue( new LightRemoveNode
			{
				Position = Chunk.Offset + position,
				Channel = 1,
				Value = GetGreenTorchLight( position )
			} );

			return SetGreenTorchLight( position, 0 );
		}

		public bool RemoveBlueTorchLight( IntVector3 position )
		{
			TorchLightRemoveQueue[2].Enqueue( new LightRemoveNode
			{
				Position = Chunk.Offset + position,
				Channel = 2,
				Value = GetBlueTorchLight( position )
			} );

			return SetBlueTorchLight( position, 0 );
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
		}

		public void UpdateTorchLight( int channel )
		{
			var removeQueue = TorchLightRemoveQueue[channel];
			var addQueue = TorchLightAddQueue[channel];

			while ( removeQueue.Count > 0 )
			{
				var node = removeQueue.Dequeue();

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( node.Position, i );
					var lightLevel = Map.GetTorchLight( neighbourPosition, channel );

					if ( lightLevel != 0 && lightLevel < node.Value )
					{
						Map.SetTorchLight( neighbourPosition, channel, 0 );

						removeQueue.Enqueue( new LightRemoveNode
						{
							Position = neighbourPosition,
							Value = node.Value
						} );
					}
					else if ( lightLevel >= node.Value )
					{
						addQueue.Enqueue( new LightAddNode
						{
							Position = neighbourPosition,
							Channel = channel
						} );
					}
				}
			}

			while ( addQueue.Count > 0 )
			{
				var node = addQueue.Dequeue();
				var lightLevel = Map.GetTorchLight( node.Position, channel );

				for ( var i = 0; i < 6; i++ )
				{
					var neighbourPosition = Map.GetAdjacentPosition( node.Position, i );
					var neighbourBlockId = Map.GetBlock( neighbourPosition );
					var neighbourBlock = Map.GetBlockType( neighbourBlockId );

					if ( Map.GetTorchLight( neighbourPosition, channel ) + 2 <= lightLevel )
					{
						if ( neighbourBlock.IsTranslucent )
						{
							Map.AddTorchLight( neighbourPosition, channel, (byte)((lightLevel - 1) * neighbourBlock.LightFilter[channel]) );
						}
					}
				}
			}
		}

		public bool UpdateTexture()
		{
			if ( IsDirty )
			{
				IsDirty = false;
				Texture.Update( Data );
				return true;
			}

			return false;
		}

		public void UpdateTorchLight()
		{
			UpdateTorchLight( 0 );
			UpdateTorchLight( 1 );
			UpdateTorchLight( 2 );
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

		public byte GetTorchLight( IntVector3 position, int channel )
		{
			if ( channel == 0 ) return GetRedTorchLight( position );
			if ( channel == 1 ) return GetGreenTorchLight( position );
			return GetBlueTorchLight( position );
		}

		public void RemoveTorchLight( IntVector3 position, int channel )
		{
			if ( channel == 0 )
				RemoveRedTorchLight( position );
			else if ( channel == 1 )
				RemoveGreenTorchLight( position );
			else
				RemoveBlueTorchLight( position );
		}

		public void AddTorchLight( IntVector3 position, int channel, byte value )
		{
			if ( channel == 0 )
				AddRedTorchLight( position, value );
			else if ( channel == 1 )
				AddGreenTorchLight( position, value );
			else
				AddBlueTorchLight( position, value );
		}

		public bool SetTorchLight( IntVector3 position, int channel, byte value )
		{
			if ( channel == 0 )
				return SetRedTorchLight( position, value );
			else if ( channel == 1 )
				return SetGreenTorchLight( position, value );
			else
				return SetBlueTorchLight( position, value );
		}

		public byte GetRedTorchLight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			if ( !IsInBounds( index ) ) return 0;
			return (byte)(Data[index] & 0xF);
		}

		public bool SetRedTorchLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			if ( !IsInBounds( index ) ) return false;
			if ( GetRedTorchLight( position ) == value ) return false;
			IsDirty = true;
			Data[index] = (byte)((Data[index] & 0xF0) | (value & 0xF));
			Data[ToIndex( position, 3 )] |= 0x40;
			return true;
		}

		public byte GetGreenTorchLight( IntVector3 position )
		{
			var index = ToIndex( position, 0 );
			if ( !IsInBounds( index ) ) return 0;
			return (byte)((Data[index] >> 4) & 0xF);
		}

		public bool SetGreenTorchLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 0 );
			if ( !IsInBounds( index ) ) return false;
			if ( GetGreenTorchLight( position ) == value ) return false;
			IsDirty = true;
			Data[index] = (byte)((Data[index] & 0x0F) | (value << 4));
			Data[ToIndex( position, 3 )] |= 0x40;
			return true;
		}

		public byte GetBlueTorchLight( IntVector3 position )
		{
			var index = ToIndex( position, 1 );
			if ( !IsInBounds( index ) ) return 0;
			return (byte)(Data[index] & 0xF);
		}

		public bool SetBlueTorchLight( IntVector3 position, byte value )
		{
			var index = ToIndex( position, 1 );
			if ( !IsInBounds( index ) ) return false;
			if ( GetBlueTorchLight( position ) == value ) return false;
			IsDirty = true;
			Data[index] = (byte)((Data[index] & 0xF0) | (value & 0xF));
			Data[ToIndex( position, 3 )] |= 0x40;
			return true;
		}
	}
}
