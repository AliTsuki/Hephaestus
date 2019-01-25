using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class containing Chunk functions
public class Chunk : ITickable
{
    // Chunk variables/objects
    private static bool FirstChunk = false;
    private bool IsFirstChunk = false;
    protected bool HasGenerated = false;
    protected bool HasDrawn = false;
    protected bool Drawnlock = false;
    private bool Renderinglock = false;
    bool NeedToUpdate = false;
    protected bool HasRendered = false;
    private readonly World world;
    private MeshData data;
    private GameObject go;
    private Block[,,] _Blocks;

    // Chunk size in blocks
    public static readonly int ChunkWidth = 20;
    public static readonly int ChunkHeight = 20;

    // Chunk position getter/setter
    public int PosX { get; private set; }
    public int PosZ { get; private set; }

    // Chunk constructor
    public Chunk(int px, int pz, World world)
    {
        this.PosX = px;
        this.PosZ = pz;
        this.world = world;
    }

    // Chunk constructor for saved data
    public Chunk(int px, int pz, int[,,] _data, World world)
    {
        this.HasGenerated = true;
        this.PosX = px;
        this.PosZ = pz;
        this.LoadChunkFromData(_data);
        this.world = world;
    }

    // Chunk Start: Generate Chunks
    public virtual void Start()
    {
        if(!FirstChunk)
        {
            FirstChunk = true;
            this.IsFirstChunk = true;
            Debug.Log("Chunk C_" + this.PosX + "_" + this.PosZ + " : IsFirstChunk = " + this.IsFirstChunk.ToString());
        }
        if(this.HasGenerated)
        {
            return;
        }
        if(this.IsFirstChunk)
        {
            Debug.Log($@"Starting Player at C_{this.PosX}_{this.PosZ} location X:{this.PosX * ChunkWidth}, Y:100, Z:{this.PosZ * ChunkWidth}");
            GameManager.instance.StartPlayer(new Vector3(this.PosX * ChunkWidth, 100, this.PosZ * ChunkWidth));
        }
        this._Blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];
        for(int x = 0; x < ChunkWidth; x++)
        {
            for(int y = 0; y < ChunkHeight; y++)
            {
                for(int z = 0; z < ChunkWidth; z++)
                {
                    float perlin = this.GetHeight(x, y, z);
                    if(perlin > GameManager.Scutoff)
                    {
                        this._Blocks[x, y, z] = Block.Air;
                    }
                    else
                    {
                        if(perlin > GameManager.Scutoff / 1.15f)
                        {
                            this._Blocks[x, y, z] = Block.Grass;
                        }
                        else if(perlin > GameManager.Scutoff / 2)
                        {
                            this._Blocks[x, y, z] = Block.Dirt;
                        }
                        else
                        {
                            this._Blocks[x, y, z] = Block.Stone;
                        }
                    }
                    if(y <= 1)
                    {
                        this._Blocks[x, y, z] = Block.Bedrock;
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
            if(!this.Drawnlock && !this.Renderinglock)
            {
                this.HasDrawn = false;
                this.HasRendered = false;
                this.NeedToUpdate = false;
            }
        }
        if(!this.HasDrawn && this.HasGenerated && !this.Drawnlock)
        {
            this.Drawnlock = true;
            this.data = new MeshData();
            for(int x = 0; x < ChunkWidth; x++)
            {
                for(int y = 0; y < ChunkHeight; y++)
                {
                    for(int z = 0; z < ChunkWidth; z++)
                    {
                        this.data.Merge(this._Blocks[x, y, z].Draw(this, this._Blocks, x, y, z));
                    }
                }
            }
            this.Drawnlock = false;
            this.HasDrawn = true;
        }
    }

    // Chunk On Unity Update: Render Chunks
    public virtual void OnUnityUpdate()
    {
        if (this.HasGenerated && !this.HasRendered && this.HasDrawn && !this.Renderinglock)
        {
            this.Renderinglock = true;
            this.HasRendered = true;
            Mesh mesh = this.data.ToMesh();
            if(this.go == null)
            {
                this.go = new GameObject
                {
                    name = "C_" + this.PosX + "_" + this.PosZ
                };
            }
            Transform t = this.go.transform;
            if(t.gameObject.GetComponent<MeshFilter>() == null)
            {
                t.gameObject.AddComponent<MeshFilter>();
                t.gameObject.AddComponent<MeshRenderer>();
                t.gameObject.AddComponent<MeshCollider>();
                t.transform.position = new Vector3(this.PosX * ChunkWidth, 0, this.PosZ * ChunkWidth);
                Texture2D Atlas = new Texture2D(0, 0, TextureFormat.RGB24, false);
                Atlas.LoadImage(System.IO.File.ReadAllBytes("Assets/Resources/Textures/Atlas/atlas.png"));
                Atlas.filterMode = FilterMode.Point;
                Atlas.wrapMode = TextureWrapMode.Repeat;
                t.gameObject.GetComponent<MeshRenderer>().material.mainTexture = Atlas;
                t.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
            }
            t.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            t.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            this.Renderinglock = false;
        }
    }

    // Get height from noise
    public float GetHeight(float px, float py, float pz)
    {
        px += this.PosX * ChunkWidth;
        pz += this.PosZ * ChunkWidth;

        float p1 = Mathf.PerlinNoise(px / GameManager.Sdx, pz / GameManager.Sdz) * GameManager.Smul;
        p1 *= GameManager.Smy * py;
        return p1;
    }

    // Set Block at position
    internal void SetBlock(int x, int y, int z, Block blocks)
    {
        this._Blocks[x, y, z] = blocks;
        this.NeedToUpdate = true;
    }

    // Degenerate Chunks
    public void Degenerate()
    {
        // First: save unloading chunks to file
        try
        {
            Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(this.PosX, this.PosZ), this.GetChunkSaveData());
            Debug.Log("Saving CHUNK to FILE: C_" + this.PosX + "_" + this.PosZ);
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        // Second: Destroy Chunk GameObject
        GameManager.instance.RegisterDelegate(new Action(() =>
        {
            GameObject.Destroy(this.go);
            Debug.Log("Degenerating CHUNK at: " + this.PosX + "_" + this.PosZ);
        }));
        // Third: Remove chunk from World
        this.world.RemoveChunk(this);
    }

    // Get ChunkData as array of Ints
    public int[,,] GetChunkSaveData()
    {
        return this._Blocks.ToIntArray();
    }

    // Get ChunkData as array of Blocks
    public void LoadChunkFromData(int[,,] _data)
    {
        this._Blocks = _data.ToBlockArray();
    }
}
