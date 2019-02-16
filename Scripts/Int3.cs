using UnityEngine;

// Class of Int3 container
public struct Int3
{
    // Int3 variables
    public int x, y, z;

    // Int3 constructor, given int x, y, z
    public Int3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Int3 constructor, given Vector3
    public Int3(Vector3 pos)
    {
        this.x = (int)pos.x;
        this.y = (int)pos.y;
        this.z = (int)pos.z;
    }

    // Int3 constructor, given Vector3Int
    public Int3(Vector3Int pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    // Overload for * operator, multiplies an Int3 by an integer
    public static Int3 operator *(Int3 multiplicand, int multiplier)
    {
        Int3 result = multiplicand;
        result.x *= multiplier;
        result.y *= multiplier;
        result.z *= multiplier;
        return result;
    }

    // Sets x, y, z position, given int x, y, z
    public void SetPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Sets x, y, z position, given Int3
    public void SetPos(Int3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    // Add x, y, z position, given Int3
    public void AddPos(Int3 pos)
    {
        this.x += pos.x;
        this.y += pos.y;
        this.z += pos.z;
    }

    // Add x, y, z position, given Int3
    public void AddPos(int x, int y, int z)
    {
        this.x += x;
        this.y += y;
        this.z += z;
    }

    // Get Chunk coords, used on World Coords
    public void ToChunkCoords()
    {
        Int3 chunkPos = new Int3(this.x / Chunk.ChunkSize, this.y / Chunk.ChunkSize, this.z / Chunk.ChunkSize);
        if(this.x < 0)
        {
            if(this.x % Chunk.ChunkSize < 0)
            {
                chunkPos.x -= 1;
            }
        }
        if(this.y < 0)
        {
            if(this.y % Chunk.ChunkSize < 0)
            {
                chunkPos.y -= 1;
            }
        }
        if(this.z < 0)
        {
            if(this.z % Chunk.ChunkSize < 0)
            {
                chunkPos.z -= 1;
            }
        }
        this.x = chunkPos.x;
        this.y = chunkPos.y;
        this.z = chunkPos.z;
    }

    // Get Internal Chunk Coords, used on World Coords
    public void ToInternalChunkCoords()
    {
        Int3 chunkBlocks = new Int3(this.x / Chunk.ChunkSize, this.y / Chunk.ChunkSize, this.z / Chunk.ChunkSize);
        if(this.x < 0)
        {
            if(this.x % Chunk.ChunkSize < 0)
            {
                chunkBlocks.x -= 1;
            }
        }
        chunkBlocks.x *= Chunk.ChunkSize;
        if(this.y < 0)
        {
            if(this.y % Chunk.ChunkSize < 0)
            {
                chunkBlocks.y -= 1;
            }
        }
        chunkBlocks.y *= Chunk.ChunkSize;
        if(this.z < 0)
        {
            if(this.z % Chunk.ChunkSize < 0)
            {
                chunkBlocks.z -= 1;
            }
        }
        chunkBlocks.z *= Chunk.ChunkSize;
        this.x = this.x - chunkBlocks.x;
        this.y = this.y - chunkBlocks.y;
        this.z = this.z - chunkBlocks.z;
    }

    // Get World Coords, used on Chunk Internal Coords, given Int3 Chunk Coords
    public void ToWorldCoords(Int3 chunkPos)
    {
        this.x = this.x + (chunkPos.x * Chunk.ChunkSize);
        this.y = this.y + (chunkPos.y * Chunk.ChunkSize);
        this.z = this.z + (chunkPos.z * Chunk.ChunkSize);
    }

    // Get Vector3 from Int3
    public Vector3 GetVec3()
    {
        Vector3 vec3 = new Vector3(this.x, this.y, this.z);
        return vec3;
    }

    // Get position as string in format "X:#, Y:#, Z:#"
    public override string ToString()
    {
        return $@"X:{this.x}, Y:{this.y}, Z:{this.z}";
    }
}
