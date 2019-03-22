using Unity.Mathematics;

public static class CoordinateMath
{
    public static int3 BlockInternalChunkCoordsToWorldCoords(int3 pos, int3 chunkPos)
    {
        pos.x = pos.x * chunkPos.x * Bootstrap.ChunkSize;
        pos.y = pos.y * chunkPos.y * Bootstrap.ChunkSize;
        pos.z = pos.z * chunkPos.z * Bootstrap.ChunkSize;
        return pos;
    }

    public static int3 WorldCoordsToChunkCoords(int3 pos)
    {
        int3 chunkPos = new int3(pos.x / Bootstrap.ChunkSize, pos.y / Bootstrap.ChunkSize, pos.z / Bootstrap.ChunkSize);
        if(pos.x < 0)
        {
            if(pos.x % Bootstrap.ChunkSize < 0)
            {
                chunkPos.x -= 1;
            }
        }
        if(pos.y < 0)
        {
            if(pos.y % Bootstrap.ChunkSize < 0)
            {
                chunkPos.y -= 1;
            }
        }
        if(pos.z < 0)
        {
            if(pos.z % Bootstrap.ChunkSize < 0)
            {
                chunkPos.z -= 1;
            }
        }
        pos.x = chunkPos.x;
        pos.y = chunkPos.y;
        pos.z = chunkPos.z;
        return pos;
    }
}
