using System;
using SharpNoise.Modules;
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
    protected bool drawnLock = false;
    private bool renderingLock = false;
    public bool NeedToUpdate = false;
    public bool HasBeenModified = false;
    public int[] NegXNeighbor;
    public int[] PosXNeighbor;
    public int[] NegZNeighbor;
    public int[] PosZNeighbor;
    public bool NeedToUpdateNegXNeighbor = false;
    public bool NeedToUpdatePosXNeighbor = false;
    public bool NeedToUpdateNegZNeighbor = false;
    public bool NeedToUpdatePosZNeighbor = false;
    private readonly World world;
    private MeshData data;
    private GameObject go;
    private Block[,,] Blocks;

    // Chunk size in blocks
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;

    // Chunk position getter/setter
    public int PosX { get; private set; }
    public int PosZ { get; private set; }

    // Chunk constructor for new chunks
    public Chunk(int px, int pz, World world)
    {
        this.PosX = px;
        this.PosZ = pz;
        this.world = world;
        this.NegXNeighbor = new int[] { this.PosX - 1, this.PosZ };
        this.PosXNeighbor = new int[] { this.PosX + 1, this.PosZ };
        this.NegZNeighbor = new int[] { this.PosX, this.PosZ - 1 };
        this.PosZNeighbor = new int[] { this.PosX, this.PosZ + 1 };
    }

    // Chunk constructor for saved data
    public Chunk(int px, int pz, int[,,] data, World world)
    {
        this.HasGenerated = true;
        this.PosX = px;
        this.PosZ = pz;
        this.LoadChunkFromData(data);
        this.world = world;
        this.NegXNeighbor = new int[] { this.PosX - 1, this.PosZ };
        this.PosXNeighbor = new int[] { this.PosX + 1, this.PosZ };
        this.NegZNeighbor = new int[] { this.PosX, this.PosZ - 1 };
        this.PosZNeighbor = new int[] { this.PosX, this.PosZ + 1 };
    }

    // Chunk Start: Generate Chunks
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
        this.Blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];
        System.Random r = new System.Random();
        int check;
        for(int x = 0; x < ChunkWidth; x++)
        {
            for(int y = 0; y < ChunkHeight; y++)
            {
                for(int z = 0; z < ChunkWidth; z++)
                {
                    float perlinNew = this.GetNoiseNew(x, y, z);
                    float perlinNewCave = this.GetNoiseNewCave(x, y, z);
                    // Above Ground Generation
                    // Air Layer
                    if(perlinNew > GameManager.STATICAirAndLandIntersectionCutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                    }
                    // Top layer
                    else if(perlinNew < GameManager.STATICAirAndLandIntersectionCutoff && perlinNew > GameManager.STATICLandTopLayerCutoff)
                    {
                        check = r.Next(-4, 4);
                        if(check + y > 110)
                        {
                            this.Blocks[x, y, z] = Block.Snow;
                        }
                        else if(check + y < 110 && check + y > 100)
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                        else if(check + y < 100 && check + y > 90)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                        }
                        else if(check + y < 90 && check + y > 62)
                        {
                            this.Blocks[x, y, z] = Block.Grass;
                        }
                        else if(check + y < 62 && check + y > 30)
                        {
                            this.Blocks[x, y, z] = Block.Sand;
                        }
                        else
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                    }
                    // Secondary Layer
                    else if(perlinNew < GameManager.STATICLandTopLayerCutoff && perlinNew > GameManager.STATICLand2NDLayerCutoff)
                    {
                        check = r.Next(-4, 4);
                        if(check + y > 100)
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                        }
                        else if(check + y < 100 && check + y > 62)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                        }
                        else if(check + y < 62 && check + y > 30)
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
                    if(perlinNewCave > GameManager.STATICCaveCutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                    }
                    // Bedrock Generation
                    check = r.Next(-1, 1);
                    if(y + check <= 2)
                    {
                        this.Blocks[x, y, z] = Block.Bedrock;
                    }
                }
            }
        }
        // Tree generation
        for(int x = 0; x < ChunkWidth; x++)
        {
            for(int z = 0; z < ChunkWidth; z++)
            {
                float perlinTree = this.GetNoiseForTree(x, z);
                if(perlinTree > GameManager.Sdcutofftreemin && perlinTree < GameManager.Sdcutofftreemax)
                {
                    int y = MathHelper.GetHighestClearBlockPositionTree(this.Blocks, x, z);
                    if(y < ChunkHeight - 7 && y > ChunkHeight * 0.3)
                    {
                        MathHelper.GenerateTree(this.Blocks, x, y, z, this.PosX, this.PosZ);
                    }
                }
            }
        }
        this.HasGenerated = true;
    }

    // Chunk Tick
    public void Tick()
    {

    }

    // Chunk Update: Draw Chunks
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
            for(int x = 0; x < ChunkWidth; x++)
            {
                for(int y = 0; y < ChunkHeight; y++)
                {
                    for(int z = 0; z < ChunkWidth; z++)
                    {
                        this.data.Merge(this.Blocks[x, y, z].Draw(this, this.Blocks, x, y, z));
                    }
                }
            }
            this.drawnLock = false;
            this.hasDrawn = true;
        }
    }

    // Chunk On Unity Update: Render Chunks
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
                    name = $@"C_{this.PosX}_{this.PosZ}"
                };
            }
            Transform cTransform = this.go.transform;
            if(cTransform.gameObject.GetComponent<MeshFilter>() == null)
            {
                cTransform.gameObject.AddComponent<MeshFilter>();
                cTransform.gameObject.AddComponent<MeshRenderer>();
                cTransform.gameObject.AddComponent<MeshCollider>();
                cTransform.transform.position = new Vector3(this.PosX * ChunkWidth, 0, this.PosZ * ChunkWidth);
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
            if(this.IsFirstChunk)
            {
                Vector3 PlayerStartPosition = World.Instance.PlayerStartingPos.GetVec3();
                PlayerStartPosition.y = MathHelper.GetHighestClearBlockPosition(this.Blocks, PlayerStartPosition.x, PlayerStartPosition.z, this.PosX, this.PosZ);
                GameManager.Instance.StartPlayer(PlayerStartPosition, this.go);
            }
        }
    }

    // Get noise
    public float GetNoiseNew(float px, float py, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;
        var ridgedMulti = new RidgedMulti()
        {
            Frequency = 0.015f,
            Lacunarity = 2f,
            OctaveCount = 4,
            Seed = 0,
        };
        var billow = new Billow()
        {
            Frequency = 0.015f,
            Lacunarity = 2f,
            OctaveCount = 4,
            Persistence = 0.5f,
            Seed = 0,
        };
        var scaleBias = new ScaleBias()
        {
            Source0 = billow,
            Scale = 0.125f,
            Bias = -0.75f,
        };
        var perlin = new Perlin()
        {
            Frequency = 0.015f,
            Lacunarity = 2f,
            OctaveCount = 4,
            Persistence = 0.25f,
            Seed = 0,
        };
        var select = new Select()
        {
            Control = perlin,
            Source0 = scaleBias,
            Source1 = ridgedMulti,
            LowerBound = 0f,
            UpperBound = 1000f,
            EdgeFalloff = 0.125f,
        };
        var noiseSource = new Turbulence()
        {
            Source0 = select,
            Frequency = 0.015f,
            Power = 0.125f,
            Roughness = 2,
            Seed = 0,
        };
        return (float)noiseSource.GetValue(px, py, pz) + ((py - (ChunkHeight * 0.3f)) * GameManager.STATICyMultiplier);
    }

    // Get noise for Cave Generation
    public float GetNoiseNewCave(float px, float py, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;
        RidgedMulti ridged = new RidgedMulti()
        {
            Frequency = GameManager.STATICRidgedFrequency,
            Lacunarity = GameManager.STATICRidgedLacunarity,
            OctaveCount = GameManager.STATICRidgedOctaveCount,
            Seed = GameManager.STATICRidgedSeed,
        };
        return (float)ridged.GetValue(px, py, pz) - ((py / (ChunkHeight * 0.5f)) * GameManager.STATICCaveyMultiplier);
    }

    // Get noise tree generation
    public float GetNoiseForTree(float px, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;
        float xz = Mathf.PerlinNoise(px / GameManager.Streedx, pz / GameManager.Streedz) * GameManager.Streemul;
        float oxz = Mathf.PerlinNoise((px / GameManager.Streendx) + GameManager.Streeoffset, (pz / GameManager.Streendz) - GameManager.Streeoffset) * GameManager.Streenmul;
        xz = (xz + oxz) / 2f;
        return xz;
    }

    // Get Block at position
    public Block GetBlock(int x, int y, int z)
    {
        Block b = this.Blocks[x, y, z];
        return b;
    }

    // Set Block at position used by player
    internal void PlayerSetBlock(int x, int y, int z, Block block)
    {
        this.Blocks[x, y, z] = block;
        this.NeedToUpdate = true;
        this.HasBeenModified = true;
        // TODO: if block modified is on chunk x/z edge, 
        // TODO: inform neighbors to update, write code to let chunks know their neighbors, if neighbors have been generated, and if neighbors need to be updated
        if(x == 0)
        {
            this.NeedToUpdateNegXNeighbor = true;
        }
        if(x == ChunkWidth - 1)
        {
            this.NeedToUpdatePosXNeighbor = true;
        }
        if(z == 0)
        {
            this.NeedToUpdateNegZNeighbor = true;
        }
        if(z == ChunkWidth - 1)
        {
            this.NeedToUpdatePosZNeighbor = true;
        }
    }

    // Set Block at position used by Structure Generator
    internal void StructureSetBlock(int x, int y, int z, Block block)
    {
        this.Blocks[x, y, z] = block;
    }

    // Degenerate Chunks
    public void Degenerate()
    {
        // First: save unloading chunks to file
        try
        {
            // Only save chunks that have been modified by player to save disk space of save files
            if(this.HasBeenModified)
            {
                Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(this.PosX, this.PosZ), this.GetChunkSaveData());
            }
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        // Second: Destroy Chunk GameObject
        GameManager.Instance.RegisterDelegate(new Action(() =>
        {
            GameObject.Destroy(this.go);
        }));
        // Third: Remove chunk from World
        this.world.RemoveChunk(this);
    }

    // Get ChunkData as array of Ints
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
