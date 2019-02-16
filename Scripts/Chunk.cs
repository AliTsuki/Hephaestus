using System;

using UnityEngine;

// Class containing Chunk functions
public class Chunk : ITickable
{
    // Chunk variables/objects
    protected bool hasGenerated = false;
    protected bool hasDrawn = false;
    protected bool hasRendered = false;
    private bool drawnLock = false;
    private bool renderingLock = false;
    public bool NeedToUpdate = false;
    public bool HasBeenModified = false;
    public Int3 NegXNeighbor;
    public Int3 PosXNeighbor;
    public Int3 NegYNeighbor;
    public Int3 PosYNeighbor;
    public Int3 NegZNeighbor;
    public Int3 PosZNeighbor;
    public bool NeedToUpdateNegXNeighbor = false;
    public bool NeedToUpdatePosXNeighbor = false;
    public bool NeedToUpdateNegYNeighbor = false;
    public bool NeedToUpdatePosYNeighbor = false;
    public bool NeedToUpdateNegZNeighbor = false;
    public bool NeedToUpdatePosZNeighbor = false;
    private MeshData data;
    public GameObject GO;
    private Block[,,] blocks;
    // Chunk size in blocks
    public static readonly int ChunkSize = 16;
    // Chunk position getter/setter
    public Int3 Pos { get; private set; }

    // Chunk constructor, given Int3
    public Chunk(Int3 pos)
    {
        this.Pos = pos;
        this.NegXNeighbor.SetPos(this.Pos.x - 1, this.Pos.y, this.Pos.z);
        this.PosXNeighbor.SetPos(this.Pos.x + 1, this.Pos.y, this.Pos.z);
        this.NegYNeighbor.SetPos(this.Pos.x, this.Pos.y - 1, this.Pos.z);
        this.PosYNeighbor.SetPos(this.Pos.x, this.Pos.y + 1, this.Pos.z);
        this.NegZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z - 1);
        this.PosZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z + 1);
    }

    // Chunk constructor for saved data, given Int3 and int 3D Array data
    public Chunk(Int3 pos, int[,,] data)
    {
        this.Pos = pos;
        this.NegXNeighbor.SetPos(this.Pos.x - 1, this.Pos.y, this.Pos.z);
        this.PosXNeighbor.SetPos(this.Pos.x + 1, this.Pos.y, this.Pos.z);
        this.NegYNeighbor.SetPos(this.Pos.x, this.Pos.y - 1, this.Pos.z);
        this.PosYNeighbor.SetPos(this.Pos.x, this.Pos.y + 1, this.Pos.z);
        this.NegZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z - 1);
        this.PosZNeighbor.SetPos(this.Pos.x, this.Pos.y, this.Pos.z + 1);
        this.hasGenerated = true;
        this.LoadChunkFromData(data);
    }

    // Chunk Start: Generate Blocks in Chunk from noise
    public virtual void Start()
    {
        if(this.hasGenerated)
        {
            return;
        }
        this.blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
        System.Random r = new System.Random();
        int cutoffMargin;
        for(int x = 0; x < ChunkSize; x++)
        {
            for(int y = 0; y < ChunkSize; y++)
            {
                for(int z = 0; z < ChunkSize; z++)
                {
                    Int3 pos = new Int3(x, y, z);
                    pos.ToWorldCoords(this.Pos);
                    float perlin = this.GetNoise(x, y, z);
                    float perlinCave = this.GetNoiseCave(x, y, z);
                    // Above Ground Generation
                    // Air Layer
                    if(perlin > GameManager.AirAndLandIntersectionCutoff)
                    {
                        this.blocks[x, y, z] = Block.Air;
                    }
                    // Top layer
                    else if(perlin < GameManager.AirAndLandIntersectionCutoff && perlin > GameManager.LandTopLayerCutoff)
                    {
                        cutoffMargin = r.Next(-4, 4);
                        if(cutoffMargin + pos.y > 110)
                        {
                            this.blocks[x, y, z] = Block.Snow;
                        }
                        else if(cutoffMargin + pos.y < 110 && cutoffMargin + pos.y > 100)
                        {
                            this.blocks[x, y, z] = Block.Stone;
                        }
                        else if(cutoffMargin + pos.y < 100 && cutoffMargin + pos.y > 90)
                        {
                            this.blocks[x, y, z] = Block.Dirt;
                        }
                        else if(cutoffMargin + pos.y < 90 && cutoffMargin + pos.y > 62)
                        {
                            this.blocks[x, y, z] = Block.Grass;
                        }
                        else if(cutoffMargin + pos.y < 62 && cutoffMargin + pos.y > 30)
                        {
                            this.blocks[x, y, z] = Block.Sand;
                        }
                        else
                        {
                            this.blocks[x, y, z] = Block.Stone;
                        }
                    }
                    // Secondary Layer
                    else if(perlin < GameManager.LandTopLayerCutoff && perlin > GameManager.Land2NDLayerCutoff)
                    {
                        cutoffMargin = r.Next(-4, 4);
                        if(cutoffMargin + pos.y > 100)
                        {
                            this.blocks[x, y, z] = Block.Stone;
                        }
                        else if(cutoffMargin + pos.y < 100 && cutoffMargin + pos.y > 62)
                        {
                            this.blocks[x, y, z] = Block.Dirt;
                        }
                        else if(cutoffMargin + pos.y < 62 && cutoffMargin + pos.y > 30)
                        {
                            this.blocks[x, y, z] = Block.Dirt;
                        }
                        else
                        {
                            this.blocks[x, y, z] = Block.Stone;
                        }
                    }
                    // Inner Layer
                    else
                    {
                        this.blocks[x, y, z] = Block.Stone;
                    }
                    // Cave Generation
                    if(perlinCave > GameManager.CaveCutoff)
                    {
                        this.blocks[x, y, z] = Block.Air;
                    }
                }
            }
        }
        // TODO: Tree gen needs overhaul
        // Tree generation
        //for(int x = 0; x < ChunkSize; x++)
        //{
        //    for(int z = 0; z < ChunkSize; z++)
        //    {
        //        float perlinTree = this.GetNoiseForTree(x, z);
        //        if(perlinTree > GameManager.Sdcutofftreemin && perlinTree < GameManager.Sdcutofftreemax)
        //        {
        //            int y = MathHelper.GetHighestClearBlockPositionTree(this.Blocks, x, z);
        //            if(y < ChunkSize - 7 && y > ChunkSize * 0.3)
        //            {
        //                MathHelper.GenerateTree(this.Blocks, x, y, z, this.Pos.x, this.Pos.y, this.Pos.z);
        //            }
        //        }
        //    }
        //}
        this.hasGenerated = true;
        Debug.Log($@"{GameManager.time}: Generated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
        Logger.Log($@"{GameManager.time}: Generated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
    }

    // Chunk Tick
    public void Tick()
    {
        // TODO: set up chunk/block tick system
        //if(this.blocks != null)
        //{
        //    for(int x = 0; x < ChunkSize; x++)
        //    {
        //        for(int y = 0; y < ChunkSize; y++)
        //        {
        //            for(int z = 0; z < ChunkSize; z++)
        //            {
        //                if(this.blocks[x, y, z] != Block.Air)
        //                {
        //                    this.blocks[x, y, z].Tick();
        //                }
        //            }
        //        }
        //    }
        //}
    }

    // Chunk Update: Create Mesh for Chunk
    public virtual void Update()
    {
        if(this.NeedToUpdate)
        {
            if(!this.drawnLock && !this.renderingLock)
            {
                this.hasDrawn = false;
                this.hasRendered = false;
                this.NeedToUpdate = false;
            }
        }
        if(!this.hasDrawn && this.hasGenerated && !this.drawnLock)
        {
            this.drawnLock = true;
            this.data = new MeshData();
            for(int x = 0; x < ChunkSize; x++)
            {
                for(int y = 0; y < ChunkSize; y++)
                {
                    for(int z = 0; z < ChunkSize; z++)
                    {
                        try
                        {
                            this.data.Merge(this.blocks[x, y, z].Draw(x, y, z, this.blocks, this.Pos));
                        }
                        catch(Exception e)
                        {
                            Debug.Log($@"{GameManager.time}: Can't Update Chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}: {e.ToString()}");
                            Logger.Log($@"{GameManager.time}: Can't Update Chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}: {e.ToString()}");
                            this.drawnLock = false;
                            goto end;
                        }
                    }
                }
            }
            this.drawnLock = false;
            this.hasDrawn = true;
            Debug.Log($@"{GameManager.time}: Meshed chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
            Logger.Log($@"{GameManager.time}: Meshed chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
        }
        end:;
    }

    // Chunk On Unity Update: Render Chunks / Create Chunk GameObjects and Assign Meshes
    public virtual void OnUnityUpdate()
    {
        if (this.hasGenerated && !this.hasRendered && this.hasDrawn && !this.renderingLock)
        {
            this.renderingLock = true;
            Mesh mesh = this.data.ToMesh();
            if(this.GO == null)
            {
                this.GO = new GameObject
                {
                    name = $@"C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}"
                };
            }
            Transform cTransform = this.GO.transform;
            if(cTransform.gameObject.GetComponent<MeshFilter>() == null)
            {
                cTransform.gameObject.AddComponent<MeshFilter>();
                cTransform.gameObject.AddComponent<MeshRenderer>();
                cTransform.gameObject.AddComponent<MeshCollider>();
                cTransform.transform.position = new Vector3(this.Pos.x * ChunkSize, this.Pos.y * ChunkSize, this.Pos.z * ChunkSize);
                Texture2D atlas = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                atlas.LoadImage(System.IO.File.ReadAllBytes("Assets/Resources/Textures/Atlas/atlas.png"));
                atlas.filterMode = FilterMode.Point;
                atlas.wrapMode = TextureWrapMode.Clamp;
                cTransform.gameObject.GetComponent<MeshRenderer>().material.mainTexture = atlas;
                cTransform.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
            }
            cTransform.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            cTransform.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            this.GO.isStatic = true;
            this.renderingLock = false;
            this.hasRendered = true;
            Debug.Log($@"{GameManager.time}: Rendered chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
            Logger.Log($@"{GameManager.time}: Rendered chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
        }
    }

    // Get noise
    private float GetNoise(float x, float y, float z)
    {
        x += this.Pos.x * ChunkSize;
        y += this.Pos.y * ChunkSize;
        z += this.Pos.z * ChunkSize;
        return (float)World.perlin.GetValue(x, y, z) + ((y - (128 * 0.3f)) * GameManager.YMultiplier);
    }

    // Get noise for Cave Generation
    private float GetNoiseCave(float x, float y, float z)
    {
        x += this.Pos.x * ChunkSize;
        y += this.Pos.y * ChunkSize;
        z += this.Pos.z * ChunkSize;
        return (float)World.ridged.GetValue(x, y, z) - (y / (128 * 0.5f) * GameManager.CaveYMultiplier);
    }

    // TODO: Tree gen needs overhaul
    // Get noise for tree generation
    //private float GetNoiseForTree(float x, float z)
    //{
    //    x += this.Pos.x * ChunkSize;
    //    z += this.Pos.z * ChunkSize;
    //    float xz = Mathf.PerlinNoise(x / GameManager.Streedx, z / GameManager.Streedz) * GameManager.Streemul;
    //    float oxz = Mathf.PerlinNoise((x / GameManager.Streendx) + GameManager.Streeoffset, (z / GameManager.Streendz) - GameManager.Streeoffset) * GameManager.Streenmul;
    //    xz = (xz + oxz) / 2f;
    //    return xz;
    //}

    // Get Block, given Chunk Internal Coords as Int3
    public Block GetBlockFromChunkInternalCoords(Int3 pos)
    {
        Block b = this.blocks[pos.x, pos.y, pos.z];
        return b;
    }

    // Set Block at position called by player, given Int3 and Block
    internal void PlayerSetBlock(Int3 pos, Block block)
    {
        this.blocks[pos.x, pos.y, pos.z] = block;
        this.NeedToUpdate = true;
        this.HasBeenModified = true;
        if(pos.x == 0)
        {
            this.NeedToUpdateNegXNeighbor = true;
        }
        if(pos.x == ChunkSize - 1)
        {
            this.NeedToUpdatePosXNeighbor = true;
        }
        if(pos.y == 0)
        {
            this.NeedToUpdateNegYNeighbor = true;
        }
        if(pos.y == ChunkSize - 1)
        {
            this.NeedToUpdatePosYNeighbor = true;
        }
        if(pos.z == 0)
        {
            this.NeedToUpdateNegZNeighbor = true;
        }
        if(pos.z == ChunkSize - 1)
        {
            this.NeedToUpdatePosZNeighbor = true;
        }
    }

    // Set Block at position called by Structure Generator, given int x, y, z and Block
    internal void StructureSetBlock(Int3 pos, Block block)
    {
        this.blocks[pos.x, pos.y, pos.z] = block;
    }

    // Degenerate Chunk
    public void Degenerate()
    {
        // First: save unloading chunks to file
        try
        {
            // Only save chunks that have been modified by player to save disk space of save files
            if(this.HasBeenModified)
            {
                Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(this.Pos), this.GetChunkSaveData());
            }
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
            Logger.Log(e);
        }
        // Second: Destroy Chunk GameObject
        GameManager.Instance.RegisterDelegate(new Action(() =>
        {
            UnityEngine.Object.Destroy(this.GO);
        }));
        // Third: Remove Chunk from World
        World.Instance.AddChunkToRemoveList(this.Pos);
        Debug.Log($@"{GameManager.time}: Degenerated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
        Logger.Log($@"{GameManager.time}: Degenerated chunk: C_{this.Pos.x}_{this.Pos.y}_{this.Pos.z}");
    }

    // Get ChunkData as array of ints
    public int[,,] GetChunkSaveData()
    {
        return this.blocks.ToIntArray();
    }

    // Get ChunkData as array of Blocks
    public void LoadChunkFromData(int[,,] data)
    {
        this.blocks = data.ToBlockArray();
    }
}
