using ProtoBuf;

using UnityEngine;


/// <summary>
/// Contains serializable versions of all game objects.
/// </summary>
public static class SaveDataObjects
{
    /// <summary>
    /// A serializable version of a Vector3Int.
    /// </summary>
    [ProtoContract]
    public struct Vector3IntSaveData
    {
        [ProtoMember(1)]
        public int x;
        [ProtoMember(2)]
        public int y;
        [ProtoMember(3)]
        public int z;

        public Vector3IntSaveData(Vector3Int v3)
        {
            this.x = v3.x;
            this.y = v3.y;
            this.z = v3.z;
        }

        public Vector3IntSaveData(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    /// <summary>
    /// A serializable version of a Vector2Int.
    /// </summary>
    [ProtoContract]
    public struct Vector2IntSaveData
    {
        [ProtoMember(1)]
        public int x;
        [ProtoMember(2)]
        public int y;

        public Vector2IntSaveData(Vector2Int v2)
        {
            this.x = v2.x;
            this.y = v2.y;
        }

        public Vector2IntSaveData(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// A serializable version of a column.
    /// </summary>
    [ProtoContract]
    public struct ColumnSaveData
    {
        [ProtoMember(1)]
        public Vector2IntSaveData ColumnPos;
        [ProtoMember(2)]
        public ChunkSaveData[] Chunks;
        [ProtoMember(3)]
        public bool HasGeneratedBlockData;

        public ColumnSaveData(Column column)
        {
            this.ColumnPos = new Vector2IntSaveData(column.ColumnPos);
            this.Chunks = new ChunkSaveData[column.Chunks.Length];
            for(int i = 0; i < column.Chunks.Length; i++)
            {
                this.Chunks[i] = new ChunkSaveData(column.Chunks[i]);
            }
            this.HasGeneratedBlockData = column.HasGeneratedBlockData;
        }
    }

    /// <summary>
    /// A serializable version of a chunk.
    /// </summary>
    [ProtoContract]
    public struct ChunkSaveData
    {
        [ProtoMember(1)]
        public Vector3IntSaveData ChunkPos;
        [ProtoMember(2)]
        public BlockSaveData[] Blocks;
        [ProtoMember(3)]
        public BlockUpdateSaveData[] UnloadedChunkBlockUpdates;
        [ProtoMember(4)]
        public bool HasUnloadedChunkBlockUpdates;
        [ProtoMember(5)]
        public bool HasGeneratedChunkData;

        public ChunkSaveData(Chunk chunk)
        {
            this.ChunkPos = new Vector3IntSaveData(chunk.ChunkPos);
            int blockArraySize = chunk.Blocks.GetLength(0);
            if(chunk.HasGeneratedChunkData == true)
            {
                this.Blocks = new BlockSaveData[blockArraySize * blockArraySize * blockArraySize];
                Block[] blockArray = chunk.Blocks.ToSingleArray();
                for(int i = 0; i < blockArray.Length; i++)
                {
                    this.Blocks[i] = new BlockSaveData(blockArray[i].ID);
                }
            }
            else
            {
                this.Blocks = new BlockSaveData[1];
            }
            if(chunk.UnloadedChunkBlockUpdates.Count > 0)
            {
                this.HasUnloadedChunkBlockUpdates = true;
                this.UnloadedChunkBlockUpdates = new BlockUpdateSaveData[chunk.UnloadedChunkBlockUpdates.Count];
                int j = 0;
                while(chunk.UnloadedChunkBlockUpdates.Count > 0)
                {
                    if(chunk.UnloadedChunkBlockUpdates.TryDequeue(out Block.BlockUpdate blockUpdate))
                    {
                        this.UnloadedChunkBlockUpdates[j] = new BlockUpdateSaveData(blockUpdate);
                        j++;
                    }
                }
            }
            else
            {
                this.HasUnloadedChunkBlockUpdates = false;
                this.UnloadedChunkBlockUpdates = new BlockUpdateSaveData[1];
            }
            this.HasGeneratedChunkData = chunk.HasGeneratedChunkData;
        }
    }

    /// <summary>
    /// A serializable version of a block.
    /// </summary>
    [ProtoContract]
    public struct BlockSaveData
    {
        [ProtoMember(1)]
        public int ID;

        public BlockSaveData(int id)
        {
            this.ID = id;
        }
    }

    /// <summary>
    /// A serializable version of a block update.
    /// </summary>
    [ProtoContract]
    public struct BlockUpdateSaveData
    {
        [ProtoMember(1)]
        public Vector3IntSaveData WorldPos;
        [ProtoMember(2)]
        public int ID;

        public BlockUpdateSaveData(Block.BlockUpdate blockUpdate)
        {
            this.WorldPos = new Vector3IntSaveData(blockUpdate.WorldPos);
            this.ID = blockUpdate.Block.ID;
        }
    }
}
