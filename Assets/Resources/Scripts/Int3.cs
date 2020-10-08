using UnityEngine;

namespace OLD
{
    // Struct of Int3 container
    public struct Int3
    {
        // Int3 fields
        public int x, y, z;

        // Int3 constructor, given int x, y, z
        public Int3(int _x, int _y, int _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        // Int3 constructor, given Vector3
        public Int3(Vector3 _pos)
        {
            this.x = (int)_pos.x;
            this.y = (int)_pos.y;
            this.z = (int)_pos.z;
        }

        // Int3 constructor, given Vector3Int
        public Int3(Vector3Int _pos)
        {
            this.x = _pos.x;
            this.y = _pos.y;
            this.z = _pos.z;
        }

        // Int3 constructor, given Int3
        public Int3(Int3 _pos)
        {
            this = _pos;
        }

        // Overload for * operator, multiplies an Int3 by an integer
        public static Int3 operator *(Int3 _multiplicand, int _multiplier)
        {
            Int3 result = _multiplicand;
            result.x *= _multiplier;
            result.y *= _multiplier;
            result.z *= _multiplier;
            return result;
        }

        // Overload for + operator, adds an Int3 to another Int3
        public static Int3 operator +(Int3 _addee, Int3 _adder)
        {
            Int3 result = _addee;
            result.x += _adder.x;
            result.y += _adder.y;
            result.z += _adder.z;
            return result;
        }

        // Sets x, y, z position, given int x, y, z
        public void SetPos(int _x, int _y, int _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        // Add x, y, z position, given int x, y, z
        public void AddPos(int _x, int _y, int _z)
        {
            this.x += _x;
            this.y += _y;
            this.z += _z;
        }

        // Get Chunk coords, used on World Coords
        public void WorldCoordsToChunkCoords()
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
        public void WorldCoordsToInternalChunkCoords()
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
        public void ChunkInternalCoordsToWorldCoords(Int3 _chunkPos)
        {
            this.x = this.x + (_chunkPos.x * Chunk.ChunkSize);
            this.y = this.y + (_chunkPos.y * Chunk.ChunkSize);
            this.z = this.z + (_chunkPos.z * Chunk.ChunkSize);
        }

        // Get World Coords at center of Chunk, used on Int3 Chunk Coords
        public void ChunkCoordsToWorldCoords()
        {
            this.x = (this.x * Chunk.ChunkSize) + (Chunk.ChunkSize / 2);
            this.y = (this.y * Chunk.ChunkSize) + (Chunk.ChunkSize / 2);
            this.z = (this.z * Chunk.ChunkSize) + (Chunk.ChunkSize / 2);
        }

        // Get Vector3 from Int3
        public Vector3 GetVec3()
        {
            return new Vector3(this.x, this.y, this.z);
        }

        // Get position as string in format "X:#, Y:#, Z:#"
        public override string ToString()
        {
            return $@"X:{this.x}, Y:{this.y}, Z:{this.z}";
        }
    }
}
