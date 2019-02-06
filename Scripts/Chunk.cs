﻿using System;

using SharpNoise.Modules;

using UnityEngine;

// Class containing Chunk functions
public class Chunk : ITickable
{
    // Chunk variables/objects
    private static bool firstChunk = false;
    public bool IsFirstChunk = false;
    public bool HasGenerated = false;
    private bool hasDrawn = false;
    private bool hasRendered = false;
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
    private GameObject go;
    private Block[,,] Blocks;

    // Chunk size in blocks
    public static readonly int ChunkSize = 16;

    // Chunk position getter/setter
    public int PosX { get; private set; }
    public int PosY { get; private set; }
    public int PosZ { get; private set; }

    // Chunk constructor for new chunks
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

    // Chunk constructor for new chunks
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

    // Chunk constructor for saved data
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

    // Chunk constructor for saved data
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
                    float perlinNew = this.GetNoiseNew(x, y, z);
                    float perlinNewCave = this.GetNoiseNewCave(x, y, z);
                    // Above Ground Generation
                    // Air Layer
                    if(perlinNew > GameManager.STATICAirAndLandIntersectionCutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                        this.Blocks[x, y, z].SetPosition(pos);
                    }
                    // Top layer
                    else if(perlinNew < GameManager.STATICAirAndLandIntersectionCutoff && perlinNew > GameManager.STATICLandTopLayerCutoff)
                    {
                        check = r.Next(-4, 4);
                        if(check + pos.y > 110)
                        {
                            this.Blocks[x, y, z] = Block.Snow;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else if(check + pos.y < 110 && check + pos.y > 100)
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else if(check + pos.y < 100 && check + pos.y > 90)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else if(check + pos.y < 90 && check + pos.y > 62)
                        {
                            this.Blocks[x, y, z] = Block.Grass;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else if(check + pos.y < 62 && check + pos.y > 30)
                        {
                            this.Blocks[x, y, z] = Block.Sand;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                    }
                    // Secondary Layer
                    else if(perlinNew < GameManager.STATICLandTopLayerCutoff && perlinNew > GameManager.STATICLand2NDLayerCutoff)
                    {
                        check = r.Next(-4, 4);
                        if(check + pos.y > 100)
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else if(check + pos.y < 100 && check + pos.y > 62)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else if(check + pos.y < 62 && check + pos.y > 30)
                        {
                            this.Blocks[x, y, z] = Block.Dirt;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                        else
                        {
                            this.Blocks[x, y, z] = Block.Stone;
                            this.Blocks[x, y, z].SetPosition(pos);
                        }
                    }
                    // Inner Layer
                    else
                    {
                        this.Blocks[x, y, z] = Block.Stone;
                        this.Blocks[x, y, z].SetPosition(pos);
                    }
                    // Cave Generation
                    if(perlinNewCave > GameManager.STATICCaveCutoff)
                    {
                        this.Blocks[x, y, z] = Block.Air;
                        this.Blocks[x, y, z].SetPosition(pos);
                    }
                    // Bedrock Generation
                    check = r.Next(-1, 1);
                    if(pos.y + check <= 2)
                    {
                        this.Blocks[x, y, z] = Block.Bedrock;
                        this.Blocks[x, y, z].SetPosition(pos);
                    }
                }
            }
        }
        // TODO: Tree Gen is broken, needs to be fixed for new vertical stacking chunks
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
        Debug.Log($@"Generated Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
        Logger.Log($@"Generated Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
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
            Debug.Log($@"Meshing Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
            Logger.Log($@"Meshing Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
            this.drawnLock = true;
            this.data = new MeshData();
            for(int x = 0; x < ChunkSize; x++)
            {
                for(int y = 0; y < ChunkSize; y++)
                {
                    for(int z = 0; z < ChunkSize; z++)
                    {
                        this.data.Merge(this.Blocks[x, y, z].Draw(x, y, z, this.Blocks));
                    }
                }
            }
            this.drawnLock = false;
            this.hasDrawn = true;
            Debug.Log($@"Meshed Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
            Logger.Log($@"Meshed Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
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
            if(this.IsFirstChunk)
            {
                Vector3 PlayerStartPosition = World.WorldInstance.PlayerStartingPos.GetVec3();
                PlayerStartPosition.y = MathHelper.GetHighestClearBlockPosition(this.Blocks, PlayerStartPosition.x, PlayerStartPosition.z, this.PosX, this.PosZ);
                GameManager.Instance.StartPlayer(PlayerStartPosition, this.go);
            }
            Debug.Log($@"Rendered Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
            Logger.Log($@"Rendered Chunk: C_{this.PosX}_{this.PosY}_{this.PosZ}");
        }
    }

    // Get noise
    private float GetNoiseNew(float x, float y, float z)
    {
        x += this.PosX * ChunkSize;
        y += this.PosY * ChunkSize;
        z += this.PosZ * ChunkSize;
        return (float)World.perlin.GetValue(x, y, z) + ((y - (ChunkSize * 0.3f)) * GameManager.STATICyMultiplier);
    }

    // Get noise for Cave Generation
    private float GetNoiseNewCave(float x, float y, float z)
    {
        x += this.PosX * ChunkSize;
        y += this.PosY * ChunkSize;
        z += this.PosZ * ChunkSize;
        return (float)World.ridged.GetValue(x, y, z) - (y / (ChunkSize * 0.5f) * GameManager.STATICCaveyMultiplier);
    }

    // Get noise tree generation
    private float GetNoiseForTree(float x, float z)
    {
        x += this.PosX * ChunkSize;
        z += this.PosZ * ChunkSize;
        float xz = Mathf.PerlinNoise(x / GameManager.Streedx, z / GameManager.Streedz) * GameManager.Streemul;
        float oxz = Mathf.PerlinNoise((x / GameManager.Streendx) + GameManager.Streeoffset, (z / GameManager.Streendz) - GameManager.Streeoffset) * GameManager.Streenmul;
        xz = (xz + oxz) / 2f;
        return xz;
    }

    // Get Block at position
    public Block GetBlockFromChunkInternalCoords(int x, int y, int z)
    {
        Block b = this.Blocks[x, y, z];
        return b;
    }

    // Get Block at position
    public Block GetBlockFromChunkInternalCoords(Int3 pos)
    {
        Block b = this.Blocks[pos.x, pos.y, pos.z];
        return b;
    }

    // Set Block at position: used by player
    internal void PlayerSetBlock(int x, int y, int z, Block block)
    {
        this.Blocks[x, y, z] = block;
        this.Blocks[x, y, z].SetPosition(x, y, z, this.PosX, this.PosY, this.PosZ);
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

    // Set Block at position: used by player
    internal void PlayerSetBlock(Int3 pos, Block block)
    {
        this.Blocks[pos.x, pos.y, pos.z] = block;
        this.Blocks[pos.x, pos.y, pos.z].SetPosition(pos.x, pos.y, pos.z, this.PosX, this.PosY, this.PosZ);
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

    // Set Block at position: used by Structure Generator
    internal void StructureSetBlock(int x, int y, int z, Block block)
    {
        this.Blocks[x, y, z] = block;
        this.Blocks[x, y, z].SetPosition(x, y, z, this.PosX, this.PosY, this.PosZ);
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
            GameObject.Destroy(this.go);
        }));
        // Third: Remove chunk from World
        World.WorldInstance.RemoveChunk(this);
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
