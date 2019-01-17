using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator
{
    public struct BlockPositions
    {
        public Block block;
        public int index;

        public BlockPositions(Block block, int index)
        {
            this.block = block;
            this.index = index;
        }
    }

    static Dictionary<Vector3Int, List<BlockPositions>> WaitingBlocks = new Dictionary<Vector3Int, List<BlockPositions>>();

    public static void GenerateWoodenPlatform(Vector3Int pos, int x, int y, int z, Block[] blocks)
    {
        for(int i = -1; i < 2; i++)
        {
            for(int j = -1; j < 2; j++)
            {
                if(IsPointWithinBounds(x + i, y, j + z))
                    blocks[((x + i) * Chunk.size.y * Chunk.size.z) + (y * Chunk.size.z) + z + j] = Block.WoodPlanks;
                else
                {


                    Vector3Int neighborChunkPos = World.WorldCoordsToChunkCoords(pos.x + i + x, pos.y + y, pos.z + j + z);

                    if (World.instance.GetChunkAt(neighborChunkPos.x, neighborChunkPos.y, neighborChunkPos.z, out Chunk chunk))
                    {
                        chunk.blocks[PositionToIndex(pos.x + i + x, pos.y + y, pos.z + j + z)] = Block.Stone;
                        continue;
                    }
                    if (WaitingBlocks.TryGetValue(neighborChunkPos, out List<BlockPositions> list))
                    {
                        list.Add(new BlockPositions(Block.WoodPlanks, PositionToIndex(pos.x + i + x, pos.y + y, pos.z + j + z)));
                    }
                    else
                    {
                        list = new List<BlockPositions>
                        {
                            new BlockPositions(Block.WoodPlanks, PositionToIndex(pos.x + i + x, pos.y + y, pos.z + j + z))
                        };

                        WaitingBlocks.Add(neighborChunkPos, list);
                    }
                }
            }
        }
    }

    public static void GetWaitingBlocks(Vector3Int pos, Block[] blocks)
    {
        //Pull off lbocks from data structure
        //Put it into blocks array at given indices

        if (WaitingBlocks.TryGetValue(pos, out List<BlockPositions> blockPositions))
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                blocks[blockPositions[i].index] = blockPositions[i].block;
            }
        }
    }

    static bool IsPointWithinBounds(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 && z < Chunk.size.z && y < Chunk.size.y && x < Chunk.size.x;
    }

    static int PositionToIndex(int x, int y, int z)
    {
        Vector3Int chunkPos = World.WorldCoordsToChunkCoords(x, y, z);

        x -= chunkPos.x;
        y -= chunkPos.y;
        z -= chunkPos.z;

        return (x * Chunk.size.y * Chunk.size.z) + (y * Chunk.size.z) + z;
    }
}
