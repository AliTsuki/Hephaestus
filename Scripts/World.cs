using System.Collections.Generic;
using System.Threading;

using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World variables/objects
    public bool IsRunning;
    private bool ranOnce = false;
    // How far to render chunks from Playerpos
    private static readonly int renderDistanceChunks = 7;
    private Thread worldThread;
    public Int3 PlayerStartingPos;
    public readonly List<Chunk> _LoadedChunks = new List<Chunk>();

    // World instance getter/setter
    public static World Instance { get; private set; }

    // Instantiate World, Register loops, set Random Player start position
    public static void Instantiate()
    {
        Instance = new World();
        MainLoopable.GetInstance().RegisterLoops(Instance);
        System.Random r = new System.Random();
        Instance.PlayerStartingPos = new Int3(r.Next(-1000, 1000), 0, r.Next(-1000, 1000));
    }

    // Start is called before the first frame update
    // Start world thread and generate Chunks in world thread
    public void Start()
    {
        this.IsRunning = true;
        this.worldThread = new Thread(() =>
        {
            Logger.Log("Initalizing world thread...");
            while(this.IsRunning)
            {
                try
                {
                    Int3 newChunkPos = new Int3(this.PlayerStartingPos.x, 0, this.PlayerStartingPos.z);
                    if(!this.ranOnce)
                    {
                        // For first time running world thread, for all Chunk Positions within Rendering Distance 
                        // check if chunk exists in file, if so get from file, if not Generate Chunk
                        int x = 0; // current position; x
                        int z = 0; // current position; y
                        int direction = 0; // current direction; 0=RIGHT, 1=DOWN, 2=LEFT, 3=UP
                        int counter = 0; // counter
                        int chainSize = 1; // chain size
                        for(int k = 1; k <= (renderDistanceChunks - 1); k++)
                        {
                            for(int j = 0; j < (k < (renderDistanceChunks - 1) ? 2 : 3); j++)
                            {
                                for(int i = 0; i < chainSize; i++)
                                {
                                    newChunkPos.SetPos(this.PlayerStartingPos.x, 0, this.PlayerStartingPos.z);
                                    newChunkPos.AddPos(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth);
                                    newChunkPos.ToChunkCoordinates();
                                    // If file exists for Chunk, read chunk data from file and add Chunk to _LoadedChunks
                                    if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos.x, newChunkPos.z)))
                                    {
                                        try
                                        {
                                            Chunk chunk = new Chunk(newChunkPos.x, newChunkPos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos.x, newChunkPos.z)), this);
                                            this._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                        catch(System.Exception e)
                                        {
                                            Debug.Log(e.ToString());
                                        }
                                    }
                                    else
                                    {
                                        Chunk chunk = new Chunk(newChunkPos.x, newChunkPos.z, this);
                                        this._LoadedChunks.Add(chunk);
                                        chunk.Start();
                                    }
                                    counter++;
                                    switch(direction)
                                    {
                                        case 0: z = z + 1; break;
                                        case 1: x = x + 1; break;
                                        case 2: z = z - 1; break;
                                        case 3: x = x - 1; break;
                                    }
                                }
                                direction = (direction + 1) % 4;
                            }
                            chainSize = chainSize + 1;
                        }
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(this.PlayerStartingPos.x, this.PlayerStartingPos.z)) <= ((renderDistanceChunks - 1) * Chunk.ChunkWidth))
                            {
                                this._LoadedChunks[i].Update();
                            }
                        }
                        this.ranOnce = true;
                    }
                    // After ran once, continuously update
                    // If Player has been loaded in, keep generating chunks around player and degenerating chunks that are too far from player
                    if(GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        for(int x = -renderDistanceChunks; x < renderDistanceChunks; x++)
                        {
                            for(int z = -renderDistanceChunks; z < renderDistanceChunks; z++)
                            {
                                newChunkPos.SetPos(currentPlayerPos.x, 0, currentPlayerPos.z);
                                newChunkPos.AddPos(x * Chunk.ChunkWidth, 0, z * Chunk.ChunkWidth);
                                newChunkPos.ToChunkCoordinates();
                                if(!this.ChunkExists(newChunkPos.x, newChunkPos.z))
                                {
                                    if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos.x, newChunkPos.z)))
                                    {
                                        try
                                        {
                                            Chunk chunk = new Chunk(newChunkPos.x, newChunkPos.z, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos.x, newChunkPos.z)), this);
                                            this._LoadedChunks.Add(chunk);
                                            chunk.Start();
                                        }
                                        catch(System.Exception e)
                                        {
                                            Debug.Log(e.ToString());
                                        }
                                    }
                                    else
                                    {
                                        Chunk chunk = new Chunk(newChunkPos.x, newChunkPos.z, this);
                                        this._LoadedChunks.Add(chunk);
                                        chunk.Start();
                                    }
                                }
                            }
                        }
                        // Loop through loaded chunks and run Chunk.Update(): Draw/Update meshes to render
                        for(int i = 0; i < this._LoadedChunks.Count; i++)
                        {
                            if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(currentPlayerPos.x, currentPlayerPos.z)) <= ((renderDistanceChunks - 1) * Chunk.ChunkWidth))
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
                            if(Vector2.Distance(new Vector2(this._LoadedChunks[i].PosX * Chunk.ChunkWidth, this._LoadedChunks[i].PosZ * Chunk.ChunkWidth), new Vector2(currentPlayerPos.x, currentPlayerPos.z)) > (renderDistanceChunks * 1.5 * Chunk.ChunkWidth))
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
        this.worldThread.Start();
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
