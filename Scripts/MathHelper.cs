using System.Collections.Generic;

using UnityEngine;

// Class of Int3 container
public class Int3
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

    // Get block position as string in format "X:#, Y:#, Z:#"
    public override string ToString()
    {
        return string.Format("X:{0}, Y:{1}, Z:{2}", this.x, this.y, this.z);
    }

    // Sets x,y,z position
    internal void SetPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Add x,y,z position
    internal void AddPos(int x, int y, int z)
    {
        this.x += x;
        this.y += y;
        this.z += z;
    }

    // Get Chunk coords from given block coords
    internal void ToChunkCoordinates()
    {
        this.x = Mathf.FloorToInt(this.x / Chunk.ChunkWidth);
        this.z = Mathf.FloorToInt(this.z / Chunk.ChunkWidth);
    }
}

// Class for Mesh maths
public class MathHelper
{
    // Draw Cube at location using Chunk, Blocks, Block, pos x,y,z and UVMaps for different sides
    // Uses Vec3 list of Vertex positions, int list of what order to draw vertices, and uvmap coords for vertices
    // Draw Grass Block
    public static MeshData DrawCube(Chunk chunk, Block[,,] _Blocks, Block block, int x, int y, int z, Vector2[] _uvmap, Vector2[] _uvmap2, Vector2[] _uvmap3)
    {
        MeshData d = new MeshData();
        // If Air don't bother looping through draw below
        if(block.Equals(Block.Air))
        {
            return new MeshData();
        }
        // Bottom Face
        if(y > 0 && _Blocks[x, y - 1, z].Istransparent())
        {
            d.Merge(new MeshData( // Bottom Face
                new List<Vector3>()
                {
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(1,0,0),
                    new Vector3(1,0,1)
                },
                new List<int>()
                {
                    0,2,1,3,1,2
                },
                new Vector2[]
                {
                    _uvmap3[0],
                    _uvmap3[1],
                    _uvmap3[2],
                    _uvmap3[3]
                }));
        }
        // Top Face
        if(y + 1 > Chunk.ChunkHeight - 1 || _Blocks[x, y + 1, z].Istransparent())
        {
            d.Merge(new MeshData( // Top Face
                new List<Vector3>()
                {
                    new Vector3(0,1,0),
                    new Vector3(0,1,1),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1)
                },
                new List<int>()
                {
                    0,1,2,3,2,1
                },
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Front Face
        if((x - 1 >= 0 && _Blocks[x - 1, y, z].Istransparent()) || 
            (x - 1 < 0 && World._instance.ChunkExists(chunk.NegXNeighbor[0], chunk.NegXNeighbor[1])
            && World._instance.GetChunk(chunk.NegXNeighbor[0], chunk.NegXNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.NegXNeighbor[0], chunk.NegXNeighbor[1]).GetBlock(Chunk.ChunkWidth - 1, y, z).Istransparent()))
        {
            d.Merge(new MeshData( // Front Face
                new List<Vector3>()
                {
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(0,1,0),
                    new Vector3(0,1,1)
                },
                new List<int>()
                {
                    0,1,2,3,2,1
                },
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        // Back Face
        if((x + 1 < Chunk.ChunkWidth && _Blocks[x + 1, y, z].Istransparent()) || 
            (x + 1 >= Chunk.ChunkWidth && World._instance.ChunkExists(chunk.PosXNeighbor[0], chunk.PosXNeighbor[1])
            && World._instance.GetChunk(chunk.PosXNeighbor[0], chunk.PosXNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.PosXNeighbor[0], chunk.PosXNeighbor[1]).GetBlock(0, y, z).Istransparent()))
        {
            d.Merge(new MeshData( // Back Face
                new List<Vector3>()
                {
                    new Vector3(1,0,0),
                    new Vector3(1,0,1),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1)
                },
                new List<int>()
                {
                    0,2,1,3,1,2
                },
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        // Left Face
        if((z - 1 >= 0 && _Blocks[x, y, z - 1].Istransparent()) ||
            (z - 1 < 0 && World._instance.ChunkExists(chunk.NegZNeighbor[0], chunk.NegZNeighbor[1])
            && World._instance.GetChunk(chunk.NegZNeighbor[0], chunk.NegZNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.NegZNeighbor[0], chunk.NegZNeighbor[1]).GetBlock(x, y, Chunk.ChunkWidth - 1).Istransparent()))
        {
            d.Merge(new MeshData( // Left Face
                new List<Vector3>()
                {
                    new Vector3(0,0,0),
                    new Vector3(1,0,0),
                    new Vector3(0,1,0),
                    new Vector3(1,1,0)
                },
                new List<int>()
                {
                    0,2,1,3,1,2
                },
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        // Right Face
        if((z + 1 < Chunk.ChunkWidth && _Blocks[x, y, z + 1].Istransparent()) ||
            (z + 1 >= Chunk.ChunkWidth && World._instance.ChunkExists(chunk.PosZNeighbor[0], chunk.PosZNeighbor[1])
            && World._instance.GetChunk(chunk.PosZNeighbor[0], chunk.PosZNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.PosZNeighbor[0], chunk.PosZNeighbor[1]).GetBlock(x, y, 0).Istransparent()))
        {
            d.Merge(new MeshData( // Right Face
                new List<Vector3>()
                {
                    new Vector3(0,0,1),
                    new Vector3(1,0,1),
                    new Vector3(0,1,1),
                    new Vector3(1,1,1)
                },
                new List<int>()
                {
                    0,1,2,3,2,1
                },
                new Vector2[]
                {
                    _uvmap2[2],
                    _uvmap2[0],
                    _uvmap2[3],
                    _uvmap2[1]
                }));
        }
        d.AddPos(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        return d;
    }

    // Draw Cube at location using Chunk, Blocks, Block, pos x,y,z and UVMap
    // Uses Vec3 list of Vertex positions, int list of what order to draw vertices, and uvmap coords for vertices
    // Draw block with all sides the same
    public static MeshData DrawCube(Chunk chunk, Block[,,] _Blocks, Block block, int x, int y, int z, Vector2[] _uvmap)
    {
        MeshData d = new MeshData();
        // If Air don't bother looping through draw below
        if(block.Equals(Block.Air))
        {
            return new MeshData();
        }
        // Bottom Face
        if(y > 0 && _Blocks[x, y - 1, z].Istransparent())
        {
            d.Merge(new MeshData( // Bottom Face
                new List<Vector3>()
                {
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(1,0,0),
                    new Vector3(1,0,1)
                },
                new List<int>()
                {
                    0,2,1,3,1,2
                },
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Top Face
        if(y + 1 > Chunk.ChunkHeight - 1 || _Blocks[x, y + 1, z].Istransparent())
        {
            d.Merge(new MeshData( // Top Face
                new List<Vector3>()
                {
                    new Vector3(0,1,0),
                    new Vector3(0,1,1),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1)
                },
                new List<int>()
                {
                    0,1,2,3,2,1
                },
                new Vector2[]
                {
                    _uvmap[0],
                    _uvmap[1],
                    _uvmap[2],
                    _uvmap[3]
                }));
        }
        // Front Face
        if((x - 1 >= 0 && _Blocks[x - 1, y, z].Istransparent()) ||
            (x - 1 < 0 && World._instance.ChunkExists(chunk.NegXNeighbor[0], chunk.NegXNeighbor[1])
            && World._instance.GetChunk(chunk.NegXNeighbor[0], chunk.NegXNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.NegXNeighbor[0], chunk.NegXNeighbor[1]).GetBlock(Chunk.ChunkWidth - 1, y, z).Istransparent()))
        {
            d.Merge(new MeshData( // Front Face
                new List<Vector3>()
                {
                    new Vector3(0,0,0),
                    new Vector3(0,0,1),
                    new Vector3(0,1,0),
                    new Vector3(0,1,1)
                },
                new List<int>()
                {
                    0,1,2,3,2,1
                },
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        // Back Face
        if((x + 1 < Chunk.ChunkWidth && _Blocks[x + 1, y, z].Istransparent()) ||
            (x + 1 >= Chunk.ChunkWidth && World._instance.ChunkExists(chunk.PosXNeighbor[0], chunk.PosXNeighbor[1])
            && World._instance.GetChunk(chunk.PosXNeighbor[0], chunk.PosXNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.PosXNeighbor[0], chunk.PosXNeighbor[1]).GetBlock(0, y, z).Istransparent()))
        {
            d.Merge(new MeshData( // Back Face
                new List<Vector3>()
                {
                    new Vector3(1,0,0),
                    new Vector3(1,0,1),
                    new Vector3(1,1,0),
                    new Vector3(1,1,1)
                },
                new List<int>()
                {
                    0,2,1,3,1,2
                },
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        // Left Face
        if((z - 1 >= 0 && _Blocks[x, y, z - 1].Istransparent()) ||
            (z - 1 < 0 && World._instance.ChunkExists(chunk.NegZNeighbor[0], chunk.NegZNeighbor[1])
            && World._instance.GetChunk(chunk.NegZNeighbor[0], chunk.NegZNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.NegZNeighbor[0], chunk.NegZNeighbor[1]).GetBlock(x, y, Chunk.ChunkWidth - 1).Istransparent()))
        {
            d.Merge(new MeshData( // Left Face
                new List<Vector3>()
                {
                    new Vector3(0,0,0),
                    new Vector3(1,0,0),
                    new Vector3(0,1,0),
                    new Vector3(1,1,0)
                },
                new List<int>()
                {
                    0,2,1,3,1,2
                },
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        // Right Face
        if((z + 1 < Chunk.ChunkWidth && _Blocks[x, y, z + 1].Istransparent()) ||
            (z + 1 >= Chunk.ChunkWidth && World._instance.ChunkExists(chunk.PosZNeighbor[0], chunk.PosZNeighbor[1])
            && World._instance.GetChunk(chunk.PosZNeighbor[0], chunk.PosZNeighbor[1]).HasGenerated
            && World._instance.GetChunk(chunk.PosZNeighbor[0], chunk.PosZNeighbor[1]).GetBlock(x, y, 0).Istransparent()))
        {
            d.Merge(new MeshData( // Right Face
                new List<Vector3>()
                {
                    new Vector3(0,0,1),
                    new Vector3(1,0,1),
                    new Vector3(0,1,1),
                    new Vector3(1,1,1)
                },
                new List<int>()
                {
                    0,1,2,3,2,1
                },
                new Vector2[]
                {
                    _uvmap[2],
                    _uvmap[0],
                    _uvmap[3],
                    _uvmap[1]
                }));
        }
        d.AddPos(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
        return d;
    }

    // Add Block to Chunk
    internal static void AddBlock(Vector3 roundedposition, Block block)
    {
        if(roundedposition.y >= Chunk.ChunkHeight)
        {
            return;
        }
        int Chunkposx = Mathf.FloorToInt(roundedposition.x / Chunk.ChunkWidth);
        int Chunkposz = Mathf.FloorToInt(roundedposition.z / Chunk.ChunkWidth);
        Chunk currentchunk;
        try
        {
            currentchunk = World._instance.GetChunk(Chunkposx, Chunkposz);
            if(currentchunk.GetType().Equals(typeof(ErroredChunk)))
            {
                Debug.Log($@"Current CHUNK is ERRORED: C_{Chunkposx}_{Chunkposz}");
                return;
            }
            int x = (int)(roundedposition.x - (Chunkposx * Chunk.ChunkWidth));
            int y = (int)roundedposition.y;
            int z = (int)(roundedposition.z - (Chunkposz * Chunk.ChunkWidth));
            currentchunk.SetBlock(x, y, z, block);
        }
        catch(System.Exception e)
        {
            Debug.Log(e.Message.ToString());
        }
    }
}
