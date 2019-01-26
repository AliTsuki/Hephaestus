using System.Collections.Generic;
using System.Threading;

using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World variables/objects
    private bool IsRunning;
    private bool RanOnce = false;
    // How far to render chunks from Playerpos
    private static readonly int RenderDistanceChunks = 5;
    private Thread worldthread;
    private Int3 PlayerStartingPos;
    public readonly List<Chunk> _LoadedChunks = new List<Chunk>();

    // World instance getter/setter
    public static World _instance { get; private set; }

    // Instantiate World, Register loops, set Random Player start position
    public static void Instantiate()
    {
        Debug.Log("World.Instantiate() executing...");
        _instance = new World();
        MainLoopable.GetInstance().RegisterLoops(_instance);
        System.Random r = new System.Random();
        _instance.PlayerStartingPos = new Int3(r.Next(-1000, 1000), 100, r.Next(-1000, 1000));
        Debug.Log($@"Starting Player Position in WORLD set to: {_instance.PlayerStartingPos.ToString()}");
    }

    // Start is called before the first frame update
    // Start world thread and generate Chunks in world thread
    public void Start()
    {
        this.IsRunning = true;
        this.worldthread = new Thread(() =>
        {
            Logger.Log("Initalizing world thread...");
            while(this.IsRunning)
            {
                try
                {
                    Int3 newchunkpos = new Int3(this.PlayerStartingPos.x, 0, this.PlayerStartingPos.z);
                    if(!this.RanOnce)
                    {
                        // For first time running world thread, for all Chunk Positions within Rendering Distance 
                        // check if chunk exists in file, if so get from file, if not Generate Chunk
                        for(int x = -RenderDistanceChunks; x < RenderDistanceChunks; x++)
                        {
                            for(int z = -RenderDistanceChunks; z < RenderDistanceChunks; z++)
                            {
                                newchunkpos.SetPos(this.PlayerStartingPos.x, 0, this.PlayerStartingPos.z);
                                newchunkpos.AddPos(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth);
                                newchunkpos.ToChunkCoordinates();
                                // If file exists for Chunk, read chunk data from file and add Chunk to _LoadedChunks
                                if(System.IO.File.Exists(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)))
                                {
                                    try
                                    {
                                        Debug.Log($@"Reading CHUNK from FILE: C_{newchunkpos.x}_{newchunkpos.z}");
                                        this._LoadedChunks.Add(new Chunk(newchunkpos.x, newchunkpos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)), this));
                                    }
                                    catch(System.Exception e)
                                    {
                                        Debug.Log(e.ToString());
                                    }
                                }
                                else
                                {
                                    Debug.Log($@"First Time Generation of CHUNK: C_{newchunkpos.x}_{newchunkpos.z}");
                                    this._LoadedChunks.Add(new Chunk(newchunkpos.x, newchunkpos.z, this));
                                }
                            }
                        }
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            Debug.Log($@"Starting CHUNK: C_{_LoadedChunks[i].PosX}_{_LoadedChunks[i].PosZ}");
                            this._LoadedChunks[i].Start();
                        }
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(this.PlayerStartingPos.x, this.PlayerStartingPos.z)) <= (RenderDistanceChunks - 1 * Chunk.ChunkWidth))
                            {
                                // TODO: FIX THIS
                                Debug.Log($@"Updating CHUNK: C_{_LoadedChunks[i].PosX}_{_LoadedChunks[i].PosZ}");
                                this._LoadedChunks[i].Update();
                            }
                        }
                        this.RanOnce = true;
                        Debug.Log("World.RanOnce in World.Start()");
                    }
                    // After ran once, continuously update
                    // If Player has been loaded in, keep generating chunks around player and degenerating chunks that are too far from player
                    if(GameManager.PlayerLoaded())
                    {
                        Int3 CurrentPlayerPos = new Int3(GameManager.instance.Playerpos);
                        for(int x = -RenderDistanceChunks; x < RenderDistanceChunks; x++)
                        {
                            for(int z = -RenderDistanceChunks; z < RenderDistanceChunks; z++)
                            {
                                newchunkpos.SetPos(CurrentPlayerPos.x, 0, CurrentPlayerPos.z);
                                newchunkpos.AddPos(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth);
                                newchunkpos.ToChunkCoordinates();
                                if(!this.ChunkExists(newchunkpos.x, newchunkpos.z))
                                {
                                    if(System.IO.File.Exists(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)))
                                    {
                                        try
                                        {
                                            Debug.Log($@"Reading CHUNK from FILE: C_{newchunkpos.x}_{newchunkpos.z}");
                                            Chunk c = new Chunk(newchunkpos.x, newchunkpos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newchunkpos.x, newchunkpos.z)), this);
                                            c.Start();
                                            this._LoadedChunks.Add(c);
                                        }
                                        catch(System.Exception e)
                                        {
                                            Debug.Log(e.ToString());
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log($@"First Time Generation of CHUNK: C_{newchunkpos.x}_{newchunkpos.z}");
                                        Chunk c = new Chunk(newchunkpos.x, newchunkpos.z, this);
                                        c.Start();
                                        this._LoadedChunks.Add(c);
                                    }
                                }
                            }
                        }
                        // Loop through loaded chunks and run Chunk.Update(): Draw/Update meshes to render
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(this.PlayerStartingPos.x, this.PlayerStartingPos.z)) <= (RenderDistanceChunks - 1 * Chunk.ChunkWidth))
                            {
                                // Before update, if chunk has been set that it's neighbors need to update, tell those neighbors they need to update
                                // Neighbors will need to update meshes if a block is changed at the intersection of chunks to ensure no extra tris are rendered unseen
                                if(this._LoadedChunks[i].NeedToUpdateNegXNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX - 1, this._LoadedChunks[i].PosZ))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX - 1, this._LoadedChunks[i].PosZ)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegXNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdatePosXNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX + 1, this._LoadedChunks[i].PosZ))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX + 1, this._LoadedChunks[i].PosZ)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdatePosXNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdateNegZNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosZ - 1))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosZ - 1)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                }
                                if(this._LoadedChunks[i].NeedToUpdatePosZNeighbor && this.ChunkExists(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosZ + 1))
                                {
                                    this._LoadedChunks[this.GetChunkIndex(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosZ + 1)].NeedToUpdate = true;
                                    this._LoadedChunks[i].NeedToUpdateNegZNeighbor = false;
                                }
                                this._LoadedChunks[i].Update();
                            }
                        }
                        // Iterate through Loaded Chunks and Degenerate if they are too far from player position
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(CurrentPlayerPos.x, CurrentPlayerPos.z)) > (RenderDistanceChunks * 2 * Chunk.ChunkWidth))
                            {
                                this._LoadedChunks[i].Degenerate();
                            }
                        }
                    }
                }
                catch(System.Exception e)
                {
                    Debug.Log(e.ToString());
                    Logger.Log(e);
                }
            }
            Logger.Log("World thread successfully stopped.");
            Logger.MainLog.Update(); // TODO: FIX IN FUTURE, BAD PRACTICE, This reruns last log
        });
        this.worldthread.Start();
    }

    // Remove Chunk from world
    internal void RemoveChunk(Chunk chunk)
    {
        this._LoadedChunks.Remove(chunk);
    }

    // Check if Chunk currently exists
    public bool ChunkExists(int posx, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return true;
            }
        }
        return false;
    }

    // Get Chunk at location
    public Chunk GetChunk(int posx, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return this._LoadedChunks[i];
            }
        }
        return new ErroredChunk(posx, posz, this);
    }

    // Get Chunk index position in Loaded Chunks
    public int GetChunkIndex(int posx, int posz)
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            if(this._LoadedChunks[i].PosX.Equals(posx) && this._LoadedChunks[i].PosZ.Equals(posz))
            {
                return i;
            }
        }
        return 0; // TODO: Fix this... ChunkExists() is always called before this, so it should never be trying to find an index for a chunk that isn't loaded... but if it does, i gots problems
    }

    // Update is called once per frame
    // Update Chunks
    public void Update()
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            this._LoadedChunks[i].OnUnityUpdate();
        }
    }

    // On Application Quit, save Chunks to file and stop world thread
    public void OnApplicationQuit()
    {
        for(int i = 0; i < this._LoadedChunks.Count; i++)
        {
            try
            {
                // Only bother saving chunks which have been modified by player
                if(this._LoadedChunks[i].HasBeenModified)
                {
                    Debug.Log($@"Saving CHUNK to FILE: C_{this._LoadedChunks[i].PosX}_{this._LoadedChunks[i].PosZ}");
                    Serializer.Serialize_ToFile_FullPath<int[,,]>(FileManager.GetChunkString(this._LoadedChunks[i].PosX, this._LoadedChunks[i].PosZ), this._LoadedChunks[i].GetChunkSaveData());
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        this.IsRunning = false;
        Logger.Log("Stopping world thread...");
    }
}
