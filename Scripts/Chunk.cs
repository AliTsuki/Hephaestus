using System;

using UnityEngine;

// Class containing Chunk functions
public class Chunk : ITickable
{
    // Chunk variables/objects
    private static bool firstChunk = false;
    public bool IsFirstChunk = false;
    public bool HasGenerated = false;
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
    public GameObject go;
    public Block[,,] Blocks;
    // Chunk size in blocks
    public static readonly int ChunkSize = 16;
    // Chunk position getter/setter
    public int PosX { get; private set; }
    public int PosY { get; private set; }
    public int PosZ { get; private set; }

    // Chunk constructor, given int x, y, z
    public Chunk(int px, int py, int pz)
    {
        this.PosX = px;
        this.PosY = py;
        this.PosZ = pz;
        this.NegXNeighbor = new Int3(this.PosX - 1, this.PosY, this.PosZ);
        this.PosXNeighbor = new Int3(this.PosX + 1, this.PosY, this.PosZ);
        this.NegYNeighbor = new Int3(this.PosX, this.PosY - 1, this.PosZ);
        this.PosYNeighbor = new Int3(this.PosX, this.PosY + 1, this.PosZ);
        this.NegZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ - 1);
        this.PosZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ + 1);
    }

    // Chunk constructor, given Int3
    public Chunk(Int3 pos)
    {
        this.PosX = pos.x;
        this.PosY = pos.y;
        this.PosZ = pos.z;
        this.NegXNeighbor = new Int3(this.PosX - 1, this.PosY, this.PosZ);
        this.PosXNeighbor = new Int3(this.PosX + 1, this.PosY, this.PosZ);
        this.NegYNeighbor = new Int3(this.PosX, this.PosY - 1, this.PosZ);
        this.PosYNeighbor = new Int3(this.PosX, this.PosY + 1, this.PosZ);
        this.NegZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ - 1);
        this.PosZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ + 1);
    }

    // Chunk constructor for saved data, given int x, y, z and int 3D Array data
    public Chunk(int px, int py, int pz, int[,,] data)
    {
        this.PosX = px;
        this.PosY = py;
        this.PosZ = pz;
        this.NegXNeighbor = new Int3(this.PosX - 1, this.PosY, this.PosZ);
        this.PosXNeighbor = new Int3(this.PosX + 1, this.PosY, this.PosZ);
        this.NegYNeighbor = new Int3(this.PosX, this.PosY - 1, this.PosZ);
        this.PosYNeighbor = new Int3(this.PosX, this.PosY + 1, this.PosZ);
        this.NegZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ - 1);
        this.PosZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ + 1);
        this.HasGenerated = true;
        this.LoadChunkFromData(data);
    }

    // Chunk constructor for saved data, given Int3 and int 3D Array data
    public Chunk(Int3 pos, int[,,] data)
    {
        this.PosX = pos.x;
        this.PosY = pos.y;
        this.PosZ = pos.z;
        this.NegXNeighbor = new Int3(this.PosX - 1, this.PosY, this.PosZ);
        this.PosXNeighbor = new Int3(this.PosX + 1, this.PosY, this.PosZ);
        this.NegYNeighbor = new Int3(this.PosX, this.PosY - 1, this.PosZ);
        this.PosYNeighbor = new Int3(this.PosX, this.PosY + 1, this.PosZ);
        this.NegZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ - 1);
        this.PosZNeighbor = new Int3(this.PosX, this.PosY, this.PosZ + 1);
        this.HasGenerated = true;
        this.LoadChunkFromData(data);
    }

    // Chunk Start: Generate Blocks in Chunk from noise
    public virtual void Start()
    {
        if(!firstChunk)
        {
            firstChunk = true;
            this.IsFirstChunk = true;
        }
        if(this.HasGenerated)
        {
            return;
        }
        this.Blocks = new Block[ChunkSize, ChunkSize, ChunkSize];
        System.Random r = new System.Random();
        int check;
        for(int x = 0; x < ChunkSize; x++)
        {
            for(int y = 0; y < ChunkSize; y++)
            {
                for(int z = 0; z < ChunkSize; z++)
                {
                    Int3 pos = new Int3(x, y, z);
                    pos.ToWorldCoords(this.PosX, this.PosY, this.PosZ);
                    float perlinNew = this.GetNoise(x, y, z);
                    float perlinNewCave = this.GetNoiseCave(x, y, z);
                    // Above Ground Generation
                    // Air Layer
                    if(perlinNew > GameManager.AirAndLandIntersectionCutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                    }
                    // Top layer
                    else if(perlinNew < GameManager.AirAndLandIntersectionCutoff && perlinNew > GameManager.LandTopLayerCutoff)
                    {
                        check = r.Next(-4, 4);
                        if(check + pos.y > 110)
                        {
                            this.Blocks[x, y, z] = Block.Snow;
                        }
                        else if(check + pos.y < 110 && check + pos.y > 100)
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                        else if(check + pos.y < 100 && check + pos.y > 90)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                        }
                        else if(check + pos.y < 90 && check + pos.y > 62)
                        {
                            this.Blocks[x, y, z] = Block.Grass;
                        }
                        else if(check + pos.y < 62 && check + pos.y > 30)
                        {
                            this.Blocks[x, y, z] = Block.Sand;
                        }
                        else
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                    }
                    // Secondary Layer
                    else if(perlinNew < GameManager.LandTopLayerCutoff && perlinNew > GameManager.Land2NDLayerCutoff)
                    {
                        check = r.Next(-4, 4);
                        if(check + pos.y > 100)
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                        else if(check + pos.y < 100 && check + pos.y > 62)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                        }
                        else if(check + pos.y < 62 && check + pos.y > 30)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                        }
                        else
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                    }
                    // Inner Layer
                    else
                    {
                        this.Blocks[x, y, z] = Block.Stone;
                    }
                    // Cave Generation
                    if(perlinNewCave > GameManager.CaveCutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
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
        //                MathHelper.GenerateTree(this.Blocks, x, y, z, this.PosX, this.PosY, this.PosZ);
        //            }
        //        }
        //    }
        //}
        this.HasGenerated = true;
        Debug.Log($@"{GameManager.time}: Generated chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
        Logger.Log($@"{GameManager.time}: Generated chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
    }

    // Chunk Tick
    public void Tick()
    {

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
        if(!this.hasDrawn && this.HasGenerated && !this.drawnLock)
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
                            this.data.Merge(this.Blocks[x, y, z].Draw(x, y, z, this.Blocks, this.PosX, this.PosY, this.PosZ));
                        }
                        catch(Exception e)
                        {
                            Debug.Log($@"{GameManager.time}: Can't Update Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}: {e.ToString()}");
                            Logger.Log($@"{GameManager.time}: Can't Update Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}: {e.ToString()}");
                            this.drawnLock = false;
                            goto end;
                        }
                    }
                }
            }
            this.drawnLock = false;
            this.hasDrawn = true;
            Debug.Log($@"{GameManager.time}: Meshed chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
            Logger.Log($@"{GameManager.time}: Meshed chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
        }
        end:;
    }

    // Chunk On Unity Update: Render Chunks / Create Chunk GameObjects and Assign Meshes
    public virtual void OnUnityUpdate()
    {
        if (this.HasGenerated && !this.hasRendered && this.hasDrawn && !this.renderingLock)
        {
            this.renderingLock = true;
            Mesh mesh = this.data.ToMesh();
            if(this.go == null)
            {
                this.go = new GameObject
                {
                    name = $@"C_{this.PosX}_{this.PosY}_{this.PosZ}"
                };
            }
            Transform cTransform = this.go.transform;
            if(cTransform.gameObject.GetComponent<MeshFilter>() == null)
            {
                cTransform.gameObject.AddComponent<MeshFilter>();
                cTransform.gameObject.AddComponent<MeshRenderer>();
                cTransform.gameObject.AddComponent<MeshCollider>();
                cTransform.transform.position = new Vector3(this.PosX * ChunkSize, this.PosY * ChunkSize, this.PosZ * ChunkSize);
                Texture2D atlas = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                atlas.LoadImage(System.IO.File.ReadAllBytes("Assets/Resources/Textures/Atlas/atlas.png"));
                atlas.filterMode = FilterMode.Point;
                atlas.wrapMode = TextureWrapMode.Clamp;
                cTransform.gameObject.GetComponent<MeshRenderer>().material.mainTexture = atlas;
                cTransform.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
            }
            cTransform.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            cTransform.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            this.renderingLock = false;
            this.hasRendered = true;
            Debug.Log($@"{GameManager.time}: Rendered chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
            Logger.Log($@"{GameManager.time}: Rendered chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
        }
    }

    // Get noise
    private float GetNoise(float x, float y, float z)
    {
        x += this.PosX * ChunkSize;
        y += this.PosY * ChunkSize;
        z += this.PosZ * ChunkSize;
        return (float)World.perlin.GetValue(x, y, z) + ((y - (128 * 0.3f)) * GameManager.YMultiplier);
    }

    // Get noise for Cave Generation
    private float GetNoiseCave(float x, float y, float z)
    {
        x += this.PosX * ChunkSize;
        y += this.PosY * ChunkSize;
        z += this.PosZ * ChunkSize;
        return (float)World.ridged.GetValue(x, y, z) - (y / (128 * 0.5f) * GameManager.CaveYMultiplier);
    }

    // TODO: Tree gen needs overhaul
    // Get noise for tree generation
    //private float GetNoiseForTree(float x, float z)
    //{
    //    x += this.PosX * ChunkSize;
    //    z += this.PosZ * ChunkSize;
    //    float xz = Mathf.PerlinNoise(x / GameManager.Streedx, z / GameManager.Streedz) * GameManager.Streemul;
    //    float oxz = Mathf.PerlinNoise((x / GameManager.Streendx) + GameManager.Streeoffset, (z / GameManager.Streendz) - GameManager.Streeoffset) * GameManager.Streenmul;
    //    xz = (xz + oxz) / 2f;
    //    return xz;
    //}

    // Get Block, given Chunk Internal Coords as int x, y, z
    public Block GetBlockFromChunkInternalCoords(int x, int y, int z)
    {
        Block b = this.Blocks[x, y, z];
        return b;
    }

    // Get Block, given Chunk Internal Coords as Int3
    public Block GetBlockFromChunkInternalCoords(Int3 pos)
    {
        Block b = this.Blocks[pos.x, pos.y, pos.z];
        return b;
    }

    // Set Block at position called by player, given int x, y, z, and Block
    internal void PlayerSetBlock(int x, int y, int z, Block block)
    {
        this.Blocks[x, y, z] = block;
        this.NeedToUpdate = true;
        this.HasBeenModified = true;
        if(x == 0)
        {
            this.NeedToUpdateNegXNeighbor = true;
        }
        if(x == ChunkSize - 1)
        {
            this.NeedToUpdatePosXNeighbor = true;
        }
        if(y == 0)
        {
            this.NeedToUpdateNegYNeighbor = true;
        }
        if(y == ChunkSize - 1)
        {
            this.NeedToUpdatePosYNeighbor = true;
        }
        if(z == 0)
        {
            this.NeedToUpdateNegZNeighbor = true;
        }
        if(z == ChunkSize - 1)
        {
            this.NeedToUpdatePosZNeighbor = true;
        }
    }

    // Set Block at position called by player, given Int3 and Block
    internal void PlayerSetBlock(Int3 pos, Block block)
    {
        this.Blocks[pos.x, pos.y, pos.z] = block;
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
    internal void StructureSetBlock(int x, int y, int z, Block block)
    {
        this.Blocks[x, y, z] = block;
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
                Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(this.PosX, this.PosY, this.PosZ), this.GetChunkSaveData());
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
            UnityEngine.Object.Destroy(this.go);
        }));
        // Third: Remove Chunk from World
        World.WorldInstance.RemoveChunk(this);
    }

    // Get ChunkData as array of ints
    public int[,,] GetChunkSaveData()
    {
        return this.Blocks.ToIntArray();
    }

    // Get ChunkData as array of Blocks
    public void LoadChunkFromData(int[,,] _data)
    {
        this.Blocks = _data.ToBlockArray();
    }
}
