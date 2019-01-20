using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class containing Chunk functions
public class Chunk : ITickable
{
    // Chunk variables
    private static bool FirstChunk = false;
    private bool IsFirstChunk = false;
    private World world;

    // Chunk size in blocks
    public static readonly int ChunkWidth = 20;
    public static readonly int ChunkHeight = 20;

    private Block[,,] _Blocks;

    // Chunk position getter
    public int PosX { get; private set; }
    public int PosZ { get; private set; }

    // Chunk constructor
    public Chunk(int px, int pz, World world)
    {
        PosX = px;
        PosZ = pz;
        this.world = world;
    }

    // Chunk constructor for saved data
    public Chunk(int px, int pz, int[,,] _data, World world)
    {
        HasGenerated = true;
        PosX = px;
        PosZ = pz;
        LoadChunkFromData(_data);
        this.world = world;
    }

    protected bool HasGenerated = false;
    
    // Get height from noise
    public float GetHeight(float px, float py, float pz)
    {
        px += (PosX * ChunkWidth);
        pz += (PosZ * ChunkWidth);

        float p1 = Mathf.PerlinNoise(px / GameManager.Sdx, pz / GameManager.Sdz) * GameManager.Smul;
        p1 *= (GameManager.Smy * py);
        return p1;
    }

    // Generate Chunks
    public virtual void Start()
    {
        if(!FirstChunk)
        {
            FirstChunk = true;
            IsFirstChunk = true;
        }
        if(HasGenerated)
            return;
        _Blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];
        for(int x = 0; x < ChunkWidth; x++)
        {
            for(int y = 0; y < ChunkHeight; y++)
            {
                for(int z = 0; z < ChunkWidth; z++)
                {
                    float perlin = GetHeight(x, y, z);
                    if(perlin > GameManager.Scutoff)
                    {
                        _Blocks[x, y, z] = Block.Air;
                    }
                    else
                    {
                        if(perlin > GameManager.Scutoff / 1.15f)
                        {
                            _Blocks[x, y, z] = Block.Grass;
                        }
                        else if(perlin > GameManager.Scutoff / 2)
                        {
                            _Blocks[x, y, z] = Block.Dirt;
                        }
                        else
                        {
                            _Blocks[x, y, z] = Block.Stone;
                        }

                    }
                    if(y <= 1)
                    {
                        _Blocks[x, y, z] = Block.Bedrock;
                    }
                }
            }
        }
        HasGenerated = true;
    }

    public void Tick()
    {

    }

    // Remove Chunks
    public void Degenerate()
    {
        // First: save unloading chunks to file
        try
        {
            Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(PosX, PosZ), GetChunkSaveData());
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        // Second: Destroy Chunk GameObject
        GameManager.instance.RegisterDelegate(new Action(() =>
        {
            GameObject.Destroy(go);
            Debug.Log(new Int3(PosX, 0, PosZ).ToString());
        }));
        // Third: Remove chunk from World
        world.RemoveChunk(this);
    }

    // Get ChunkData as array of Ints
    public int[,,] GetChunkSaveData()
    {
        return _Blocks.ToIntArray();
    }

    // Get ChunkData as array of Blocks
    public void LoadChunkFromData(int[,,] _data)
    {
        _Blocks = _data.ToBlockArray();
    }

    protected bool HasDrawn = false;
    private MeshData data;
    protected bool Drawnlock = false;
    bool NeedToUpdate = false;

    // Draw Chunks
    public virtual void Update()
    {
        if(NeedToUpdate)
        {
            if(!Drawnlock && !Renderinglock)
            {
                HasDrawn = false;
                HasRendered = false;
                NeedToUpdate = false;
            }
        }

        if(!HasDrawn && HasGenerated && !Drawnlock)
        {
            Drawnlock = true;
            data = new MeshData();
            for(int x = 0; x < ChunkWidth; x++)
            {
                for(int y = 0; y < ChunkHeight; y++)
                {
                    for(int z = 0; z < ChunkWidth; z++)
                    {
                        data.Merge(_Blocks[x, y, z].Draw(this, _Blocks, x, y, z));
                    }
                }
            }
            Drawnlock = false;
            HasDrawn = true;
        }
    }

    protected bool HasRendered = false;
    private GameObject go;
    private bool Renderinglock = false;

    // Render Chunks
    public virtual void OnUnityUpdate()
    {
        if(HasGenerated && !HasRendered && HasDrawn && !Renderinglock)
        {
            Renderinglock = true;
            HasRendered = true;
            Mesh mesh = data.ToMesh();
            if(go == null)
            {
                go = new GameObject();
            }
            Transform t = go.transform;
            if(t.gameObject.GetComponent<MeshFilter>() == null)
            {
                t.gameObject.AddComponent<MeshFilter>();
                t.gameObject.AddComponent<MeshRenderer>();
                t.gameObject.AddComponent<MeshCollider>();
                t.transform.position = new Vector3(PosX * ChunkWidth, 0, PosZ * ChunkWidth);
                Texture2D tmp = new Texture2D(0, 0);
                tmp.LoadImage(System.IO.File.ReadAllBytes("atlas.png"));
                tmp.filterMode = FilterMode.Point;
                t.gameObject.GetComponent<MeshRenderer>().material.mainTexture = tmp;
                t.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.0f);
            }
            t.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
            t.transform.GetComponent<MeshCollider>().sharedMesh = mesh;
            Renderinglock = false;
            if(IsFirstChunk)
            {
                GameManager.instance.StartPlayer(new Vector3(PosX * ChunkWidth, 100, PosZ * ChunkWidth));
            }
        }
    }

    // Set Block at position
    internal void SetBlock(int x, int y, int z, Block blocks)
    {
        _Blocks[x, y, z] = blocks;
        NeedToUpdate = true;
    }
}
