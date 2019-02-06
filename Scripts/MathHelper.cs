using System;
using System.Collections.Generic;

using UnityEngine;

// Class of Int3 container
public struct Int3
{
    // Int3 variables
    public int x, y, z;

    // Int3 constructor for Ints
    public Int3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Int3 constructor for Vec3s
    public Int3(Vector3 pos)
    {
        this.x = (int)pos.x;
        this.y = (int)pos.y;
        this.z = (int)pos.z;
    }

    // Int3 constructor for Vec3Ints
    public Int3(Vector3Int pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    // Sets x,y,z position
    public void SetPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Sets x,y,z position
    public void SetPos(Int3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    // Add x,y,z position
    public void AddPos(int x, int y, int z)
    {
        this.x += x;
        this.y += y;
        this.z += z;
    }

    // Add x,y,z position
    public void AddPos(Int3 pos)
    {
        this.x += pos.x;
        this.y += pos.y;
        this.z += pos.z;
    }

    // Get Chunk coords from given World coords
    public void ToChunkCoords()
    {
        this.x = Mathf.FloorToInt(this.x / Chunk.ChunkSize);
        this.y = Mathf.FloorToInt(this.y / Chunk.ChunkSize);
        this.z = Mathf.FloorToInt(this.z / Chunk.ChunkSize);
    }

    // Get Internal Chunk coords from World coords
    public void ToInternalChunkCoords()
    {
        this.x = Math.Abs(this.x - (Mathf.FloorToInt(this.x / Chunk.ChunkSize) * Chunk.ChunkSize));
        this.y = Math.Abs(this.y - (Mathf.FloorToInt(this.y / Chunk.ChunkSize) * Chunk.ChunkSize));
        this.z = Math.Abs(this.z - (Mathf.FloorToInt(this.z / Chunk.ChunkSize) * Chunk.ChunkSize));
    }

    // Get World coords from Within-Chunk coords by passing Chunk coords
    public void ToWorldCoords(int PosX, int PosY, int PosZ)
    {
        this.x = this.x + (PosX * Chunk.ChunkSize);
        this.y = this.y + (PosY * Chunk.ChunkSize);
        this.z = this.z + (PosZ * Chunk.ChunkSize);
    }

    // Get World coords from Within-Chunk coords by passing Chunk coords
    public void ToWorldCoords(Int3 chunkPos)
    {
        this.x = this.x + (chunkPos.x * Chunk.ChunkSize);
        this.y = this.y + (chunkPos.y * Chunk.ChunkSize);
        this.z = this.z + (chunkPos.z * Chunk.ChunkSize);
    }

    // Get Vec3 from Int3
    public Vector3 GetVec3()
    {
        Vector3 vec3 = new Vector3(this.x, this.y, this.z);
        return vec3;
    }

    // Get block position as string in format "X:#, Y:#, Z:#"
    public override string ToString()
    {
        return $@"X:{this.x}, Y:{this.y}, Z:{this.z}";
    }
}

// Class for Mesh maths
public static class MathHelper
{
    // List of Face Vertices and Draw Orders
    private static readonly List<Vector3> BottomFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        new Vector3(1,0,0),
        new Vector3(1,0,1)
    };
    private static readonly List<int> BottomFaceDrawOrder = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> TopFaceVerts = new List<Vector3>()
    {
        new Vector3(0,1,0),
        new Vector3(0,1,1),
        new Vector3(1,1,0),
        new Vector3(1,1,1)
    };
    private static readonly List<int> TopFaceDrawOrder = new List<int>()
    {
        0,1,2,3,2,1
    };
    private static readonly List<Vector3> FrontFaceVerts = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        new Vector3(0,1,0),
        new Vector3(0,1,1)
    };
    private static readonly List<int> FrontFaceDrawOrder = new List<int>()
    {
        0,1,2,3,2,1
    };
    private static readonly List<Vector3> BackFaceVerts = new List<Vector3>()
    {
        new Vector3(1,0,0),
        new Vector3(1,0,1),
        new Vector3(1,1,0),
        new Vector3(1,1,1)
    };
    private static readonly List<int> BackFaceDrawOrder = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> LeftFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(1,1,0)
    };
    private static readonly List<int> LeftFaceDrawOrder = new List<int>()
    {
        0,2,1,3,1,2
    };
    private static readonly List<Vector3> RightFaceVerts = new List<Vector3>()
    {
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(0,1,1),
        new Vector3(1,1,1)
    };
    private static readonly List<int> RightFaceDrawOrder = new List<int>()
    {
        0,1,2,3,2,1
    };
    // End of list of Face Vertices and Draw Orders

    // Draw Cube at location using Chunk, Blocks, Block, pos x,y,z and UVMaps for different sides
    // Uses Vec3 list of Vertex positions, int list of what order to draw vertices, and uvmap coords for vertices
    // Draw Grass Block
    public static MeshData DrawCubeGrass(int x, int y, int z, Block[,,] blocks, Block block, Vector2[] _uvmap, Vector2[] _uvmap2, Vector2[] _uvmap3)
    {
        MeshData data = new MeshData();
        // If Air don't bother looping through draw below
        if(block.Equals(Block.Air))
        {
            return data;
        }
        Int3 position = block.Position;
        bool blockNegXVis = CheckNegXVis(x, y, z, position, blocks);
        bool blockPosXVis = CheckPosXVis(x, y, z, position, blocks);
        bool blockNegYVis = CheckNegYVis(x, y, z, position, blocks);
        bool blockPosYVis = CheckPosYVis(x, y, z, position, blocks);
        bool blockNegZVis = CheckNegZVis(x, y, z, position, blocks);
        bool blockPosZVis = CheckPosZVis(x, y, z, position, blocks);
        // Bottom Face
        if(blockNegYVis)
        {
            data.Merge(new MeshData( // Bottom Face
                BottomFaceVerts,
                BottomFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap3[0],
                    _uvmap3[1],
                    _uvmap3[2],
                    _uvmap3[3]
                }));
        }
        // Top Face
        if(blockPosYVis)
        {
            data.Merge(new MeshData( // Top Face
                TopFaceVerts,
                TopFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Front Face
        if(blockNegXVis)
        {
            if(blockPosYVis)
            {
                data.Merge(new MeshData( // Front Face
                FrontFaceVerts,
                FrontFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
            }
            else
            {
                data.Merge(new MeshData( // Front Face
                FrontFaceVerts,
                FrontFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap3[2],
                    _uvmap3[0],
                    _uvmap3[3],
                    _uvmap3[1]
                }));
            }
        }
        // Back Face
        if(blockPosXVis)
        {
            if(blockPosYVis)
            {
                data.Merge(new MeshData( // Back Face
                BackFaceVerts,
                BackFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
            }
            else
            {
                data.Merge(new MeshData( // Back Face
                BackFaceVerts,
                BackFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap3[2],
                    _uvmap3[0],
                    _uvmap3[3],
                    _uvmap3[1]
                }));
            }
        }
        // Left Face
        if(blockNegZVis)
        {
            if(blockPosYVis)
            {
                data.Merge(new MeshData( // Left Face
                LeftFaceVerts,
                LeftFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
            }
            else
            {
                data.Merge(new MeshData( // Left Face
                LeftFaceVerts,
                LeftFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap3[2],
                    _uvmap3[0],
                    _uvmap3[3],
                    _uvmap3[1]
                }));
            }
        }
        // Right Face
        if(blockPosZVis)
        {
            if(blockPosYVis)
            {
                data.Merge(new MeshData( // Right Face
                RightFaceVerts,
                RightFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
            }
            else
            {
                data.Merge(new MeshData( // Right Face
                RightFaceVerts,
                RightFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap3[2],
                    _uvmap3[0],
                    _uvmap3[3],
                    _uvmap3[1]
                }));
            }
        }
        data.AddPos(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        return data;
    }

    // Draw Cube at location using Chunk, Blocks, Block, pos x,y,z and UVMaps for different sides
    // Uses Vec3 list of Vertex positions, int list of what order to draw vertices, and uvmap coords for vertices
    // Draw Log Block
    public static MeshData DrawCubeLogs(int x, int y, int z, Block[,,] blocks, Block block, Vector2[] _uvmap, Vector2[] _uvmap2)
    {
        MeshData data = new MeshData();
        // If Air don't bother looping through draw below
        if(block.Equals(Block.Air))
        {
            return data;
        }
        Int3 position = block.Position;
        bool blockNegXVis = CheckNegXVis(x, y, z, position, blocks);
        bool blockPosXVis = CheckPosXVis(x, y, z, position, blocks);
        bool blockNegYVis = CheckNegYVis(x, y, z, position, blocks);
        bool blockPosYVis = CheckPosYVis(x, y, z, position, blocks);
        bool blockNegZVis = CheckNegZVis(x, y, z, position, blocks);
        bool blockPosZVis = CheckPosZVis(x, y, z, position, blocks);
        // Bottom Face
        if(blockNegYVis)
        {
            data.Merge(new MeshData( // Bottom Face
                BottomFaceVerts,
                BottomFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Top Face
        if(blockPosYVis)
        {
            data.Merge(new MeshData( // Top Face
                TopFaceVerts,
                TopFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Front Face
        if(blockNegXVis)
        {
            data.Merge(new MeshData( // Front Face
                FrontFaceVerts,
                FrontFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        // Back Face
        if(blockPosXVis)
        {
            data.Merge(new MeshData( // Back Face
                BackFaceVerts,
                BackFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        // Left Face
        if(blockNegZVis)
        {
            data.Merge(new MeshData( // Left Face
                LeftFaceVerts,
                LeftFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        // Right Face
        if(blockPosZVis)
        {
            data.Merge(new MeshData( // Right Face
                RightFaceVerts,
                RightFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        data.AddPos(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        return data;
    }

    // Draw Cube at location using Chunk, Blocks, Block, pos x,y,z and UVMap
    // Uses Vec3 list of Vertex positions, int list of what order to draw vertices, and uvmap coords for vertices
    // Draw block with all sides the same
    public static MeshData DrawCube(int x, int y, int z, Block[,,] blocks, Block block, Vector2[] _uvmap)
    {
        MeshData data = new MeshData();
        // If Air don't bother looping through draw below
        if(block.Equals(Block.Air))
        {
            return data;
        }
        Int3 position = block.Position;
        Debug.Log($@"Checking Faces of Block: {x}, {y}, {z}");
        Debug.Log($@"At World Pos: {position.x}, {position.y}, {position.z}");
        Logger.Log($@"Checking Faces of Block: {x}, {y}, {z}");
        Logger.Log($@"At World Pos: {position.x}, {position.y}, {position.z}");
        bool blockNegXVis = CheckNegXVis(x, y, z, position, blocks);
        bool blockPosXVis = CheckPosXVis(x, y, z, position, blocks);
        bool blockNegYVis = CheckNegYVis(x, y, z, position, blocks);
        bool blockPosYVis = CheckPosYVis(x, y, z, position, blocks);
        bool blockNegZVis = CheckNegZVis(x, y, z, position, blocks);
        bool blockPosZVis = CheckPosZVis(x, y, z, position, blocks);
        // Bottom Face
        if(blockNegYVis)
        {
            data.Merge(new MeshData( // Bottom Face
                BottomFaceVerts,
                BottomFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Top Face
        if(blockPosYVis)
        {
            data.Merge(new MeshData( // Top Face
                TopFaceVerts,
                TopFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Front Face
        if(blockNegXVis)
        {
            data.Merge(new MeshData( // Front Face
                FrontFaceVerts,
                FrontFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        // Back Face
        if(blockPosXVis)
        {
            data.Merge(new MeshData( // Back Face
                BackFaceVerts,
                BackFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        // Left Face
        if(blockNegZVis)
        {
            data.Merge(new MeshData( // Left Face
                LeftFaceVerts,
                LeftFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        // Right Face
        if(blockPosZVis)
        {
            data.Merge(new MeshData( // Right Face
                RightFaceVerts,
                RightFaceDrawOrder,
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        data.AddPos(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        return data;
    }

    // Checks if NegX face is visible
    private static bool CheckNegXVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(x > 0)
        {
            return blocks[x - 1, y, z].IsTransparent || blocks[x - 1, y, z].IsSemiTransparent;
        }
        else if(x == 0)
        {
            return World.WorldInstance.GetBlockFromWorldCoords(position.x - 1, position.y, position.z).IsTransparent || World.WorldInstance.GetBlockFromWorldCoords(position.x - 1, position.y, position.z).IsSemiTransparent;
        }
        return false;
    }

    // Checks if PosX face is visible
    private static bool CheckPosXVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(x < Chunk.ChunkSize - 1)
        {
            return blocks[x + 1, y, z].IsTransparent || blocks[x + 1, y, z].IsSemiTransparent;
        }
        else if(x == Chunk.ChunkSize - 1)
        {
            return World.WorldInstance.GetBlockFromWorldCoords(position.x + 1, position.y, position.z).IsTransparent || World.WorldInstance.GetBlockFromWorldCoords(position.x + 1, position.y, position.z).IsSemiTransparent;
        }
        return false;
    }

    // Checks if NegY face is visible
    private static bool CheckNegYVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(y > 0)
        {
            return blocks[x, y - 1, z].IsTransparent || blocks[x, y - 1, z].IsSemiTransparent;
        }
        else if(y == 0)
        {
            return World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y - 1, position.z).IsTransparent || World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y - 1, position.z).IsSemiTransparent;
        }
        return false;
    }

    // Checks if PosY face is visible
    private static bool CheckPosYVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(y < Chunk.ChunkSize - 1)
        {
            return blocks[x, y + 1, z].IsTransparent || blocks[x, y + 1, z].IsSemiTransparent;
        }
        else if(y == Chunk.ChunkSize - 1)
        {
            return World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y + 1, position.z).IsTransparent || World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y + 1, position.z).IsSemiTransparent;
        }
        return false;
    }

    // Checks if NegZ face is visible
    private static bool CheckNegZVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(z > 0)
        {
            return blocks[x, y, z - 1].IsTransparent || blocks[x, y, z - 1].IsSemiTransparent;
        }
        else if(z == 0)
        {
            return World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y, position.z - 1).IsTransparent || World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y, position.z - 1).IsSemiTransparent;
        }
        return false;
    }

    // Checks if PosZ face is visible
    private static bool CheckPosZVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(z < Chunk.ChunkSize - 1)
        {
            return blocks[x, y, z + 1].IsTransparent || blocks[x, y, z + 1].IsSemiTransparent;
        }
        else if(z == Chunk.ChunkSize - 1)
        {
            return World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y, position.z + 1).IsTransparent || World.WorldInstance.GetBlockFromWorldCoords(position.x, position.y, position.z + 1).IsSemiTransparent;
        }
        return false;
    }

    // Add Block to Chunk
    public static void AddBlock(Vector3 position, Block block)
    {
        Int3 chunkPos = new Int3(position);
        chunkPos.ToChunkCoords();
        Chunk currentchunk;
        try
        {
            currentchunk = World.WorldInstance.GetChunk(chunkPos);
            if(currentchunk.GetType().Equals(typeof(ErroredChunk)))
            {
                Debug.Log($@"Current CHUNK is ERRORED: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
                return;
            }
            Int3 pos = new Int3(position);
            pos.ToInternalChunkCoords();
            currentchunk.PlayerSetBlock(pos, block);
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
            Logger.Log(e);
        }
    }

    // Get highest clear block: for player start position
    public static float GetHighestClearBlockPosition(Block[,,] blocks, float x, float z, int ChunkPosX, int ChunkPosZ)
    {
        float y = Chunk.ChunkSize - 1;
        int newx = (int)System.Math.Abs(x - (ChunkPosX * Chunk.ChunkSize));
        int newz = (int)System.Math.Abs(z - (ChunkPosZ * Chunk.ChunkSize));
        for(int i = Chunk.ChunkSize - 2; i >= 1; i--)
        {
            if(blocks[newx, i, newz].IsTransparent && blocks[newx, i + 1, newz].IsTransparent && !blocks[newx, i - 1, newz].IsTransparent)
            { 
                y = i + 3.7f;
                return y;
            }
        }
        return y;
    }

    // Get highest clear block: for tree generation
    public static int GetHighestClearBlockPositionTree(Block[,,] blocks, int x, int z)
    {
        int y = Chunk.ChunkSize - 1;
        for(int i = Chunk.ChunkSize - 7; i >= 1; i--)
        {
            if(blocks[x, i, z].IsTransparent && blocks[x, i + 1, z].IsTransparent && blocks[x, i + 2, z].IsTransparent
                && blocks[x, i + 3, z].IsTransparent && blocks[x, i + 4, z].IsTransparent && blocks[x, i + 5, z].IsTransparent
                && blocks[x, i + 6, z].IsTransparent && !blocks[x, i - 1, z].IsTransparent && !blocks[x, i - 1, z].Equals(Block.Leaves) && !blocks[x, i - 1, z].Equals(Block.Logs))
            {
                y = i;
                return y;
            }
            if(!blocks[x, i, z].IsTransparent && !blocks[x, i + 1, z].IsTransparent)
            {
                break;
            }
        }
        return 0;
    }

    // Generate tree at position
    public static void GenerateTree(Block[,,] blocks, int x, int y, int z, int ChunkPosX, int ChunkPosY, int ChunkPosZ)
    {
        Chunk currentchunk;
        currentchunk = World.WorldInstance.GetChunk(ChunkPosX, ChunkPosY, ChunkPosZ);
        if(x - 2 > 0 && x + 2 < Chunk.ChunkSize - 1 && z - 2 > 0 && z + 2 < Chunk.ChunkSize - 1)
        {
            currentchunk.StructureSetBlock(x, y, z, Block.Logs);
            currentchunk.StructureSetBlock(x, y + 1, z, Block.Logs);
            currentchunk.StructureSetBlock(x, y + 2, z, Block.Logs);
            currentchunk.StructureSetBlock(x, y + 3, z, Block.Logs);
            currentchunk.StructureSetBlock(x, y + 4, z, Block.Logs);
            currentchunk.StructureSetBlock(x, y + 5, z, Block.Logs);
            currentchunk.StructureSetBlock(x - 1, y + 5, z, Block.Leaves);
            currentchunk.StructureSetBlock(x + 1, y + 5, z, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 5, z - 1, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 5, z + 1, Block.Leaves);
            currentchunk.StructureSetBlock(x - 1, y + 5, z - 1, Block.Leaves);
            currentchunk.StructureSetBlock(x - 1, y + 5, z + 1, Block.Leaves);
            currentchunk.StructureSetBlock(x + 1, y + 5, z - 1, Block.Leaves);
            currentchunk.StructureSetBlock(x + 1, y + 5, z + 1, Block.Leaves);
            currentchunk.StructureSetBlock(x - 2, y + 5, z, Block.Leaves);
            currentchunk.StructureSetBlock(x + 2, y + 5, z, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 5, z - 2, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 5, z + 2, Block.Leaves);
            currentchunk.StructureSetBlock(x - 2, y + 5, z - 2, Block.Leaves);
            currentchunk.StructureSetBlock(x - 2, y + 5, z + 2, Block.Leaves);
            currentchunk.StructureSetBlock(x + 2, y + 5, z - 2, Block.Leaves);
            currentchunk.StructureSetBlock(x + 2, y + 5, z + 2, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 6, z, Block.Logs);
            currentchunk.StructureSetBlock(x - 1, y + 6, z, Block.Leaves);
            currentchunk.StructureSetBlock(x + 1, y + 6, z, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 6, z - 1, Block.Leaves);
            currentchunk.StructureSetBlock(x, y + 6, z + 1, Block.Leaves);
            currentchunk.StructureSetBlock(x - 1, y + 6, z - 1, Block.Leaves);
            currentchunk.StructureSetBlock(x - 1, y + 6, z + 1, Block.Leaves);
            currentchunk.StructureSetBlock(x + 1, y + 6, z - 1, Block.Leaves);
            currentchunk.StructureSetBlock(x + 1, y + 6, z + 1, Block.Leaves);
        }
    }
}
