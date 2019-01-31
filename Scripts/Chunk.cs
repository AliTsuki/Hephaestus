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
        for(int x = 0; x < ChunkWidth; x++)
        {
            for(int y = 0; y < ChunkHeight; y++)
            {
                for(int z = 0; z < ChunkWidth; z++)
                {
                    float perlin = this.GetNoise(x, y, z);
                    float perlinCaves = this.GetNoiseForCaves(x, y, z);
                    float perlinMountainStone = this.GetNoiseForMountainStone(x, y, z);
                    // Above Ground Generation
                    if(perlin > GameManager.Scutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                    }
                    else if(perlin > GameManager.Scutoff / GameManager.Sdcutoffgrass)
                    {
                        this.Blocks[x, y, z] = Block.Grass;
                    }
                    else if(perlin > GameManager.Scutoff / GameManager.Sdcutoffdirt)
                    {
                        this.Blocks[x, y, z] = Block.Dirt;
                    }
                    else
                    {
                        this.Blocks[x, y, z] = Block.Stone;
                    }
                    // Set stone on mountainsides
                    if(perlinMountainStone > GameManager.Smcutoff && !this.Blocks[x, y, z].Istransparent())
                    {
                        this.Blocks[x, y, z] = Block.Stone;
                    }
                    // Set dirt in stone
                    if(perlinMountainStone > GameManager.SDirtMinCutoff && perlinMountainStone < GameManager.SDirtMaxCutoff && !this.Blocks[x, y, z].Istransparent())
                    {
                        this.Blocks[x, y, z] = Block.Dirt;
                    }
                    // Cave Generation
                    if(perlinCaves > GameManager.Scavecutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                    }
                    // Bedrock Generation
                    if(y <= 1)
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

    // Get noise for terrain generation
    public float GetNoise(float px, float py, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;
        float xy = Mathf.PerlinNoise(px / GameManager.Sdx, py / GameManager.Sdy) * GameManager.Smul;
        float yz = Mathf.PerlinNoise(py / GameManager.Sdy, pz / GameManager.Sdz) * GameManager.Smul;
        float xz = Mathf.PerlinNoise(px / GameManager.Sdx, pz / GameManager.Sdz) * GameManager.Smul;
        float xyz = (xy + yz + xz) / 3f;
        float oxy = Mathf.PerlinNoise((px / GameManager.Sndx) + GameManager.Soffset, (py / GameManager.Sndy) - GameManager.Soffset) * GameManager.Snmul;
        float oyz = Mathf.PerlinNoise((py / GameManager.Sndy) - GameManager.Soffset, (pz / GameManager.Sndz) + GameManager.Soffset) * GameManager.Snmul;
        float oxz = Mathf.PerlinNoise((px / GameManager.Sndx) + GameManager.Soffset, (pz / GameManager.Sndz) - GameManager.Soffset) * GameManager.Snmul;
        float oxyz = (oxy + oyz + oxz) / 3f;
        xyz = (xyz + oxyz) / 2f;
        xyz = xyz * GameManager.Smy * py;
        return xyz;
    }

    // Get noise for cave generation
    public float GetNoiseForCaves(float px, float py, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;
        float xy = Mathf.PerlinNoise(px / GameManager.Scavedx, py / GameManager.Scavedy) * GameManager.Scavemul;
        float yz = Mathf.PerlinNoise(py / GameManager.Scavedy, pz / GameManager.Scavedz) * GameManager.Scavemul;
        float xz = Mathf.PerlinNoise(px / GameManager.Scavedx, pz / GameManager.Scavedz) * GameManager.Scavemul;
        float xyz = (xy + yz + xz) / 3f;
        float oxy = Mathf.PerlinNoise((px / GameManager.Scavendx) + GameManager.Scaveoffset, (py / GameManager.Scavendy) - GameManager.Scaveoffset) * GameManager.Scavenmul;
        float oyz = Mathf.PerlinNoise((py / GameManager.Scavendy) - GameManager.Scaveoffset, (pz / GameManager.Scavendz) + GameManager.Scaveoffset) * GameManager.Scavenmul;
        float oxz = Mathf.PerlinNoise((px / GameManager.Scavendx) + GameManager.Scaveoffset, (pz / GameManager.Scavendz) - GameManager.Scaveoffset) * GameManager.Scavenmul;
        float oxyz = (oxy + oyz + oxz) / 3f;
        xyz = (xyz + oxyz) / 2f;
        return xyz;
    }

    // Get noise for putting stone on mountain tops
    public float GetNoiseForMountainStone(float px, float py, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;
        float xy = Mathf.PerlinNoise(px / GameManager.Smdx, py / GameManager.Smdy) * GameManager.Smmul;
        float yz = Mathf.PerlinNoise(py / GameManager.Smdy, pz / GameManager.Smdz) * GameManager.Smmul;
        float xz = Mathf.PerlinNoise(px / GameManager.Smdx, pz / GameManager.Smdz) * GameManager.Smmul;
        float xyz = (xy + yz + xz) / 3f;
        float oxy = Mathf.PerlinNoise((px / GameManager.Smndx) + GameManager.Smoffset, (py / GameManager.Smndy) - GameManager.Smoffset) * GameManager.Smnmul;
        float oyz = Mathf.PerlinNoise((py / GameManager.Smndy) - GameManager.Smoffset, (pz / GameManager.Smndz) + GameManager.Smoffset) * GameManager.Smnmul;
        float oxz = Mathf.PerlinNoise((px / GameManager.Smndx) + GameManager.Smoffset, (pz / GameManager.Smndz) - GameManager.Smoffset) * GameManager.Smnmul;
        float oxyz = (oxy + oyz + oxz) / 3f;
        xyz = (xyz + oxyz) / 2f;
        xyz = xyz * GameManager.Smmy * py;
        return xyz;
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
