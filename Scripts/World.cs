using System.Collections.Generic;
using System.Threading;

using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World fields
    public bool IsRunning;
    private bool initialChunkStartComplete = false;
    private bool initialChunkUpdateComplete = false;
    private bool chunkBufferNeedsUpdate = false;
    private bool chunkBufferNeedsPropogation = false;
    private bool chunkBufferLock = false;
    private bool loadedChunksInitialized = false;
    private Thread worldGenThread;
    private Thread worldUpdateThread;
    public Int3 WorldStartPos;
    private Dictionary<Int3, Chunk> LoadedChunks = new Dictionary<Int3, Chunk>();
    private List<Chunk> ChunkBuffer = new List<Chunk>();
    private List<Chunk> LoadedChunksForUpdate = new List<Chunk>();
    private List<Int3> chunksToRemove = new List<Int3>();
    // Chunks to render around player, FirstPass is initialization, renderDistance is during regular gameplay
    public readonly int RenderDistanceFirstPass = 4;
    private readonly int renderDistance = 3;
    public static World Instance { get; private set; }

    // Instantiate World, Register loops, set Random World start position
    public static void Instantiate()
    {
        Instance = new World();
        MainLoopable.Instance.RegisterLoops(Instance);
        System.Random r = new System.Random();
        Instance.WorldStartPos = new Int3(r.Next(-1000, 1000), 62, r.Next(-1000, 1000));
    }

    // Start is called before the first frame update
    // Start world threads
    public void Start()
    {
        this.IsRunning = true;
        // Thread for generating and degenerating chunks
        this.worldGenThread = 
        new Thread(() =>
        {
            Logger.Log($@"{GameManager.time}: Initalizing world gen thread...");
            while(this.IsRunning)
            {
                try
                {
                    // For first time running world gen thread, for all Chunks within Rendering Distance First Pass
                    // Check if chunk exists in save file, if so get from file, if not Generate Chunk
                    Int3 startingChunkPos = this.WorldStartPos;
                    startingChunkPos.ToChunkCoords();
                    Int3 xyz = new Int3(0, 0, 0);
                    if(!this.initialChunkStartComplete && !GameManager.PlayerLoaded())
                    {
                        for(int x = -this.RenderDistanceFirstPass; x < this.RenderDistanceFirstPass; x++)
                        {
                            for(int y = -this.RenderDistanceFirstPass; y < this.RenderDistanceFirstPass; y++)
                            {
                                for(int z = -this.RenderDistanceFirstPass; z < this.RenderDistanceFirstPass; z++)
                                {
                                    xyz.SetPos(x, y, z);
                                    Int3 newChunkPos = this.WorldStartPos;
                                    newChunkPos += xyz * Chunk.ChunkSize;
                                    newChunkPos.ToChunkCoords();
                                    // x,y,z for loop makes a cube of chunks of renderDistanceFirstPass^3, distance function below cuts that into a smaller sphere, saves 30+% in generation time
                                    if(Vector3.Distance(newChunkPos.GetVec3(), startingChunkPos.GetVec3()) <= this.RenderDistanceFirstPass)
                                    {
                                        // If save file exists for Chunk, read chunk data from file and add Chunk to World
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                                this.LoadedChunks.Add(newChunkPos, chunk);
                                                this.chunkBufferNeedsUpdate = true;
                                                chunk.Start();
                                            }
                                            catch(System.Exception e)
                                            {
                                                Debug.Log(e.ToString());
                                                Logger.Log(e);
                                            }
                                        }
                                        // If no save file for chunk, generate new Chunk and add to World
                                        else
                                        {
                                            Chunk chunk = new Chunk(newChunkPos);
                                            this.LoadedChunks.Add(newChunkPos, chunk);
                                            this.chunkBufferNeedsUpdate = true;
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        this.initialChunkStartComplete = true;
                        Debug.Log($@"{GameManager.time}: Finished Starter Zone Generation Initialization");
                        Logger.Log($@"{GameManager.time}: Finished Starter Zone Generation Initialization");
                    }
                    // After initial chunk generatio and player loaded into world, continuously update around player
                    // If Player has been loaded in, keep generating chunks around player and degenerating chunks that are too far from player
                    if(this.initialChunkStartComplete && GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        for(int x = -this.RenderDistanceFirstPass; x < this.RenderDistanceFirstPass; x++)
                        {
                            for(int y = -this.RenderDistanceFirstPass; y < this.RenderDistanceFirstPass; y++)
                            {
                                for(int z = -this.RenderDistanceFirstPass; z < this.RenderDistanceFirstPass; z++)
                                {
                                    xyz.SetPos(x, y, z);
                                    Int3 newChunkPos = currentPlayerPos;
                                    newChunkPos += xyz * Chunk.ChunkSize;
                                    newChunkPos.ToChunkCoords();
                                    // If currently pointed to Chunk hasn't already been added to World
                                    if(!this.ChunkExists(newChunkPos))
                                    {
                                        // If save file exists for Chunk, read chunk data from file and add Chunk to World
                                        if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                        {
                                            try
                                            {
                                                Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                                this.LoadedChunks.Add(newChunkPos, chunk);
                                                this.chunkBufferNeedsUpdate = true;
                                                chunk.Start();
                                            }
                                            catch(System.Exception e)
                                            {
                                                Debug.Log(e.ToString());
                                                Logger.Log(e);
                                            }
                                        }
                                        // If no save file for chunk, generate new Chunk and add to World
                                        else
                                        {
                                            Chunk chunk = new Chunk(newChunkPos);
                                            this.LoadedChunks.Add(newChunkPos, chunk);
                                            this.chunkBufferNeedsUpdate = true;
                                            chunk.Start();
                                        }
                                    }
                                }
                            }
                        }
                        // Iterate through Loaded Chunks and Degenerate if they are too far from current player position
                        foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                        {
                            Int3 chunkPos = chunk.Value.Pos * Chunk.ChunkSize;
                            if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) >= (this.renderDistance * 1.5 * Chunk.ChunkSize))
                            {
                                if(!this.chunksToRemove.Contains(chunk.Key))
                                {
                                    chunk.Value.Degenerate();
                                }
                            }
                        }
                        this.RemoveChunks();
                    }
                    if(this.chunkBufferNeedsUpdate && !this.chunkBufferLock)
                    {
                        this.chunkBufferLock = true;
                        this.ChunkBuffer.Clear();
                        foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                        {
                            this.ChunkBuffer.Add(chunk.Value);
                        }
                        this.chunkBufferNeedsPropogation = true;
                        this.chunkBufferNeedsUpdate = false;
                        this.chunkBufferLock = false;
                    }
                }
                catch(System.Exception e)
                {
                    Debug.Log(e.ToString());
                    Logger.Log(e);
                }
            }
            Logger.Log($@"{GameManager.time}: World gen thread successfully stopped.");
        });
        // Thread for updating chunks
        this.worldUpdateThread =
        new Thread(() =>
        {
            Logger.Log($@"{GameManager.time}: Initalizing world update thread...");
            while(this.IsRunning)
            {
                try
                {
                    // After initial Chunk generation, and before player is instantiated, update all Chunks that have existing neighbors
                    // Neighbor existence is required to avoid problems in meshing the edges of the Chunk when neighbors don't exist yet
                    if(!this.initialChunkUpdateComplete && this.loadedChunksInitialized && this.initialChunkStartComplete && !GameManager.PlayerLoaded())
                    {
                        foreach(Chunk chunk in this.LoadedChunksForUpdate)
                        {
                            if(this.ChunkExists(chunk.NegXNeighbor) && this.ChunkExists(chunk.PosXNeighbor)
                                && this.ChunkExists(chunk.NegYNeighbor) && this.ChunkExists(chunk.PosYNeighbor)
                                && this.ChunkExists(chunk.NegZNeighbor) && this.ChunkExists(chunk.PosZNeighbor))
                            {
                                chunk.Update();
                            }
                        }
                        this.initialChunkUpdateComplete = true;
                        Debug.Log($@"{GameManager.time}: Finished Starter Zone Meshing Initialization");
                        Logger.Log($@"{GameManager.time}: Finished Starter Zone Meshing Initialization");
                    }
                    // After initial Chunks have been generated and meshed, try instantiating the player character
                    if(this.initialChunkStartComplete && this.initialChunkUpdateComplete && !GameManager.PlayerLoaded())
                    {
                        Int3 PlayerStartPos = MathHelper.GetPlayerStartPosition(this.WorldStartPos);
                        Int3 PlayerStartChunkPos = PlayerStartPos;
                        PlayerStartChunkPos.ToChunkCoords();
                        Chunk chunk = this.GetChunk(PlayerStartChunkPos);
                        GameObject go = chunk.GO;
                        GameManager.Instance.StartPlayer(PlayerStartPos.GetVec3(), go);
                    }
                    // After initial Chunk gen and meshing, and player start, continuously update loaded Chunks around current player position
                    if(this.initialChunkStartComplete &&  this.initialChunkUpdateComplete && GameManager.PlayerLoaded())
                    {
                        Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                        foreach(Chunk chunk in this.LoadedChunksForUpdate)
                        {
                            Int3 chunkPos = chunk.Pos * Chunk.ChunkSize;
                            if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) <= (this.renderDistance * Chunk.ChunkSize))
                            {
                                if(this.ChunkExists(chunk.NegXNeighbor) && this.ChunkExists(chunk.PosXNeighbor)
                                && this.ChunkExists(chunk.NegYNeighbor) && this.ChunkExists(chunk.PosYNeighbor)
                                && this.ChunkExists(chunk.NegZNeighbor) && this.ChunkExists(chunk.PosZNeighbor))
                                {
                                    // Before update, if Chunk has been set that its neighbors need to update, tell those neighbors they need to update
                                    // Neighbors will need to update meshes if a block is changed at the intersection of chunks to avoid holes between chunks
                                    if(chunk.NeedToUpdateNegXNeighbor)
                                    {
                                        this.GetChunk(chunk.NegXNeighbor).NeedToUpdate = true;
                                        chunk.NeedToUpdateNegXNeighbor = false;
                                    }
                                    if(chunk.NeedToUpdatePosXNeighbor)
                                    {
                                        this.GetChunk(chunk.PosXNeighbor).NeedToUpdate = true;
                                        chunk.NeedToUpdatePosXNeighbor = false;
                                    }
                                    if(chunk.NeedToUpdateNegYNeighbor)
                                    {
                                        this.GetChunk(chunk.NegYNeighbor).NeedToUpdate = true;
                                        chunk.NeedToUpdateNegYNeighbor = false;
                                    }
                                    if(chunk.NeedToUpdatePosYNeighbor)
                                    {
                                        this.GetChunk(chunk.PosYNeighbor).NeedToUpdate = true;
                                        chunk.NeedToUpdatePosYNeighbor = false;
                                    }
                                    if(chunk.NeedToUpdateNegZNeighbor)
                                    {
                                        this.GetChunk(chunk.NegZNeighbor).NeedToUpdate = true;
                                        chunk.NeedToUpdateNegZNeighbor = false;
                                    }
                                    if(chunk.NeedToUpdatePosZNeighbor)
                                    {
                                        this.GetChunk(chunk.PosZNeighbor).NeedToUpdate = true;
                                        chunk.NeedToUpdateNegZNeighbor = false;
                                    }
                                    chunk.Update();
                                }
                            }
                        }
                    }
                    if(this.chunkBufferNeedsPropogation && !this.chunkBufferLock)
                    {
                        this.chunkBufferLock = true;
                        this.LoadedChunksForUpdate.Clear();
                        foreach(Chunk chunk in this.ChunkBuffer)
                        {
                            this.LoadedChunksForUpdate.Add(chunk);
                        }
                        if(!this.loadedChunksInitialized)
                        {
                            this.loadedChunksInitialized = true;
                        }
                        this.chunkBufferNeedsPropogation = false;
                        this.chunkBufferLock = false;
                    }
                }
                catch(System.Exception e)
                {
                    Debug.Log(e.ToString());
                    Logger.Log(e);
                }
            }
            Logger.Log($@"{GameManager.time}: World update thread successfully stopped.");
        });
        // Start threads defined above
        this.worldGenThread.Start();
        this.worldUpdateThread.Start();
    }
    
    // Update is called once per frame
    // Update Chunks
    public void Update()
    {
        foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
        {
            chunk.Value.OnUnityUpdate();
        }
        Logger.MainLog.Update();
    }

    // On Application Quit, save Chunks to file and stop world threads
    public void OnApplicationQuit()
    {
        foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
        {
            try
            {
                Int3 chunkPos = chunk.Value.Pos;
                // Only save chunks which have been modified by player
                if(chunk.Value.HasBeenModified)
                {
                    Serializer.Serialize_ToFile_FullPath(FileManager.GetChunkString(chunkPos), chunk.Value.GetChunkSaveData());
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
        }
        this.IsRunning = false;
        Logger.Log($@"{GameManager.time}: Stopping world threads...");
    }

    // Remove Chunk from world, given Chunk Position
    public void AddChunkToRemoveList(Int3 chunkPos)
    {
        this.chunksToRemove.Add(chunkPos);
        Debug.Log($@"{GameManager.time}: Added Chunk to remove list: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
        Logger.Log($@"{GameManager.time}: Added Chunk to remove list: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
    }

    // Remove Chunk from World
    private void RemoveChunks()
    {
        if(this.chunksToRemove.Count > 0)
        {
            foreach(Int3 chunkPos in this.chunksToRemove)
            {
                this.LoadedChunks.Remove(chunkPos);
                Debug.Log($@"{GameManager.time}: Removed Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
                Logger.Log($@"{GameManager.time}: Removed Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
            }
            this.chunksToRemove.Clear();
        }
    }

    // Check if Chunk currently exists, given Chunk Coords
    public bool ChunkExists(Int3 chunkPos)
    {
        if(this.LoadedChunks.ContainsKey(chunkPos))
        {
            return true;
        }
        return false;
    }

    // Get Chunk at location, given Chunk Coords
    public Chunk GetChunk(Int3 chunkPos)
    {
        if(this.LoadedChunks.ContainsKey(chunkPos))
        {
            return this.LoadedChunks[chunkPos];
        }
        return new ErroredChunk(chunkPos);
    }

    // Get Block, given World Coords
    public Block GetBlockFromWorldCoords(Int3 pos)
    {
        Int3 chunkPos = pos;
        chunkPos.ToChunkCoords();
        Int3 internalPos = pos;
        internalPos.ToInternalChunkCoords();
        if(this.ChunkExists(chunkPos))
        {
            Block b = this.GetChunk(chunkPos).GetBlockFromChunkInternalCoords(internalPos);
            return b;
        }
        Debug.Log($@"{GameManager.time}: Chunk: {chunkPos.ToString()} does not exist.");
        Logger.Log($@"{GameManager.time}: Chunk: {chunkPos.ToString()} does not exist.");
        throw new System.Exception("Trying to get Blocks from Chunk that doesn't exist: ");
    }
}
