using System.Collections.Generic;

using UnityEngine;

// Class for Mesh maths
public static class MathHelper
{
    // List of Face Vertices and Triangle Draw Orders
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
    // End of list of Face Vertices and Triangle Draw Orders

    // Create Mesh for Cube, given Chunk Internal Coords as int(x,y,z), Block 3D Array blocks, Block block, UVMaps as Vector2 Arrays, and Chunk Coords as int(PosX, PosY, PosZ)
    // Draw Grass Block
    public static MeshData DrawCubeGrass(int x, int y, int z, Block[,,] blocks, Block block, Vector2[] _uvmap, Vector2[] _uvmap2, Vector2[] _uvmap3, int PosX, int PosY, int PosZ)
    {
        MeshData data = new MeshData();
        Int3 position = new Int3(x, y, z);
        position.ToWorldCoords(PosX, PosY, PosZ);
        bool blockNegXVis = CheckNegXVis(x, y, z, position, blocks);
        bool blockPosXVis = CheckPosXVis(x, y, z, position, blocks);
        bool blockNegYVis = CheckNegYVis(x, y, z, position, blocks);
        bool blockPosYVis = CheckPosYVis(x, y, z, position, blocks);
        bool blockNegZVis = CheckNegZVis(x, y, z, position, blocks);
        bool blockPosZVis = CheckPosZVis(x, y, z, position, blocks);
        // Bottom Face
        if(blockNegYVis)
        {
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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
                data.Merge(new MeshData(
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

    // Create Mesh for Cube, given Chunk Internal Coords as int(x,y,z), Block 3D Array blocks, Block block, UVMaps as Vector2 Arrays, and Chunk Coords as int(PosX, PosY, PosZ)
    // Draw Log Block
    public static MeshData DrawCubeLogs(int x, int y, int z, Block[,,] blocks, Block block, Vector2[] _uvmap, Vector2[] _uvmap2, int PosX, int PosY, int PosZ)
    {
        MeshData data = new MeshData();
        Int3 position = new Int3(x, y, z);
        position.ToWorldCoords(PosX, PosY, PosZ);
        bool blockNegXVis = CheckNegXVis(x, y, z, position, blocks);
        bool blockPosXVis = CheckPosXVis(x, y, z, position, blocks);
        bool blockNegYVis = CheckNegYVis(x, y, z, position, blocks);
        bool blockPosYVis = CheckPosYVis(x, y, z, position, blocks);
        bool blockNegZVis = CheckNegZVis(x, y, z, position, blocks);
        bool blockPosZVis = CheckPosZVis(x, y, z, position, blocks);
        // Bottom Face
        if(blockNegYVis)
        {
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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

    // Create Mesh for Cube, given Chunk Internal Coords as int(x,y,z), Block 3D Array blocks, Block block, UVMap as Vector2 Array, and Chunk Coords as int(PosX, PosY, PosZ)
    // Draw block with all sides the same
    public static MeshData DrawCube(int x, int y, int z, Block[,,] blocks, Block block, Vector2[] _uvmap, int PosX, int PosY, int PosZ)
    {
        MeshData data = new MeshData();
        Int3 position = new Int3(x, y, z);
        position.ToWorldCoords(PosX, PosY, PosZ);
        bool blockNegXVis = CheckNegXVis(x, y, z, position, blocks);
        bool blockPosXVis = CheckPosXVis(x, y, z, position, blocks);
        bool blockNegYVis = CheckNegYVis(x, y, z, position, blocks);
        bool blockPosYVis = CheckPosYVis(x, y, z, position, blocks);
        bool blockNegZVis = CheckNegZVis(x, y, z, position, blocks);
        bool blockPosZVis = CheckPosZVis(x, y, z, position, blocks);
        // Bottom Face
        if(blockNegYVis)
        {
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            data.Merge(new MeshData(
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
            Block b = blocks[x - 1, y, z];
            return b.IsTransparent || b.IsSemiTransparent;
        }
        else if(x == 0)
        {
            Block b = World.Instance.GetBlockFromWorldCoords(position.x - 1, position.y, position.z);
            return b.IsTransparent || b.IsSemiTransparent;
        }
        return false;
    }

    // Checks if PosX face is visible
    private static bool CheckPosXVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(x < Chunk.ChunkSize - 1)
        {
            Block b = blocks[x + 1, y, z];
            return b.IsTransparent || b.IsSemiTransparent;
        }
        else if(x == Chunk.ChunkSize - 1)
        {
            Block b = World.Instance.GetBlockFromWorldCoords(position.x + 1, position.y, position.z);
            return b.IsTransparent || b.IsSemiTransparent;
        }
        return false;
    }

    // Checks if NegY face is visible
    private static bool CheckNegYVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(y > 0)
        {
            Block b = blocks[x, y - 1, z];
            return b.IsTransparent || b.IsSemiTransparent;
        }
        else if(y == 0)
        {
            Block b = World.Instance.GetBlockFromWorldCoords(position.x, position.y - 1, position.z);
            return b.IsTransparent || b.IsSemiTransparent;
        }
        return false;
    }

    // Checks if PosY face is visible
    private static bool CheckPosYVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(y < Chunk.ChunkSize - 1)
        {
            Block b = blocks[x, y + 1, z];
            return b.IsTransparent || b.IsSemiTransparent;
        }
        else if(y == Chunk.ChunkSize - 1)
        {
            Block b = World.Instance.GetBlockFromWorldCoords(position.x, position.y + 1, position.z);
            return b.IsTransparent || b.IsSemiTransparent;
        }
        return false;
    }

    // Checks if NegZ face is visible
    private static bool CheckNegZVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(z > 0)
        {
            Block b = blocks[x, y, z - 1];
            return b.IsTransparent || b.IsSemiTransparent;
        }
        else if(z == 0)
        {
            Block b = World.Instance.GetBlockFromWorldCoords(position.x, position.y, position.z - 1);
            return b.IsTransparent || b.IsSemiTransparent;
        }
        return false;
    }

    // Checks if PosZ face is visible
    private static bool CheckPosZVis(int x, int y, int z, Int3 position, Block[,,] blocks)
    {
        if(z < Chunk.ChunkSize - 1)
        {
            Block b = blocks[x, y, z + 1];
            return b.IsTransparent || b.IsSemiTransparent;
        }
        else if(z == Chunk.ChunkSize - 1)
        {
            Block b = World.Instance.GetBlockFromWorldCoords(position.x, position.y, position.z + 1);
            return b.IsTransparent || b.IsSemiTransparent;
        }
        return false;
    }

    // Add Block to Chunk, used by Player, given World Coords as Vector3 and Block
    public static void AddBlock(Vector3 position, Block block)
    {
        Int3 chunkPos = new Int3(position);
        chunkPos.ToChunkCoords();
        Chunk currentchunk;
        try
        {
            currentchunk = World.Instance.GetChunk(chunkPos);
            if(currentchunk.GetType().Equals(typeof(ErroredChunk)))
            {
                Debug.Log($@"{GameManager.time}: Current CHUNK is ERRORED: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
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

    // Return highest chunk loaded at that X, Z position by checking chunks above recursively
    public static Int3 GetHighestChunk(Int3 chunkpos)
    {
        Int3 highestchunk = chunkpos;
        Int3 chunkabove = new Int3(chunkpos.x, chunkpos.y + 1, chunkpos.z);
        if(World.Instance.ChunkExists(chunkabove))
        {
            highestchunk = GetHighestChunk(chunkabove);
        }
        return highestchunk;
    }

    // Get highest clear block position for the player start
    public static Int3 GetPlayerStartPosition(Int3 worldstartpos)
    {
        Int3 playerstartpos = worldstartpos;
        for(int i = (Chunk.ChunkSize * World.Instance.renderDistanceFirstPass) - 3; i >= 0; i--)
        {
            if(!World.Instance.GetBlockFromWorldCoords(worldstartpos.x, i, worldstartpos.z).IsTransparent && World.Instance.GetBlockFromWorldCoords(worldstartpos.x, i + 1, worldstartpos.z).IsTransparent && World.Instance.GetBlockFromWorldCoords(worldstartpos.x, i + 2, worldstartpos.z).IsTransparent)
            {
                playerstartpos.SetPos(playerstartpos.x, i + 1, playerstartpos.z);
                return playerstartpos;
            }
        }
        System.Random r = new System.Random();
        playerstartpos.SetPos(GetPlayerStartPosition(new Int3(r.Next(-20, 20) + playerstartpos.x, worldstartpos.y, r.Next(-20, 20) + playerstartpos.z)));
        return playerstartpos;
    }

    // TODO: Tree Gen needs overhaul
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

    // TODO: Tree gen needs overhaul
    // Generate tree at position
    public static void GenerateTree(Block[,,] blocks, int x, int y, int z, int ChunkPosX, int ChunkPosY, int ChunkPosZ)
    {
        Chunk currentchunk;
        currentchunk = World.Instance.GetChunk(ChunkPosX, ChunkPosY, ChunkPosZ);
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
