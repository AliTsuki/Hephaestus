using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

// Class containing World functions
public class World : ILoopable
{
    // World fields
    public bool IsRunning;
    private bool starterChunksInitialized = false;
    public Int3 WorldStartPos { get; private set; }
    private readonly Dictionary<Int3, Chunk> LoadedChunks = new Dictionary<Int3, Chunk>();
    private readonly List<Int3> chunksToRemove = new List<Int3>();
    // Chunks to render around player, FirstPass is initialization, renderDistance is during regular gameplay
    public static int RenderDistanceFirstPass = 4;
    private static readonly int renderDistance = 3;
    private readonly float distanceToDegenerateMultiplier = 2;
    public static readonly TaskFactory taskFactory = new TaskFactory();
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
    public void Start()
    {
        this.IsRunning = true;
        if(this.IsRunning)
        {
            try
            {
                // Starting zone Chunk initialization, for all Chunks within Rendering Distance First Pass
                // Check if chunk exists in save file, if so get from file, if not Generate new Chunk
                Int3 startingChunkPos = this.WorldStartPos;
                startingChunkPos.WorldCoordsToChunkCoords();
                Int3 xyz = new Int3(0, 0, 0);
                for(int x = -RenderDistanceFirstPass; x < RenderDistanceFirstPass; x++)
                {
                    for(int y = -RenderDistanceFirstPass; y < RenderDistanceFirstPass; y++)
                    {
                        for(int z = -RenderDistanceFirstPass; z < RenderDistanceFirstPass; z++)
                        {
                            xyz.SetPos(x, y, z);
                            Int3 newChunkPos = this.WorldStartPos;
                            newChunkPos += xyz * Chunk.ChunkSize;
                            newChunkPos.WorldCoordsToChunkCoords();
                            // x,y,z for loop makes a cube of chunks of renderDistanceFirstPass^3, distance function below cuts that into a sphere (instead of cube), saves 30+% in generation time
                            if(Vector3.Distance(newChunkPos.GetVec3(), startingChunkPos.GetVec3()) <= RenderDistanceFirstPass)
                            {
                                // If save file exists for Chunk, read chunk data from file and add Chunk to World
                                if(System.IO.File.Exists(FileManager.GetChunkString(newChunkPos)))
                                {
                                    try
                                    {
                                        Chunk chunk = new Chunk(newChunkPos, Serializer.Deserialize_From_File<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                        this.LoadedChunks.Add(newChunkPos, chunk);
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
                                }
                            }
                        }
                    }
                }
                // Generate blocks for each of the loaded Chunks
                foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                {
                    chunk.Value.Start();
                }
                Debug.Log($@"{GameManager.time}: Finished Starter Zone Generation Initialization");
                Logger.Log($@"{GameManager.time}: Finished Starter Zone Generation Initialization");
                // Generate mesh for each of the loaded Chunks (check if they have all neighbors meshed to avoid broken meshes at Chunk intersections)
                foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                {
                    if(this.ChunkExists(chunk.Value.NegXNeighbor) && this.ChunkExists(chunk.Value.PosXNeighbor)
                        && this.ChunkExists(chunk.Value.NegYNeighbor) && this.ChunkExists(chunk.Value.PosYNeighbor)
                        && this.ChunkExists(chunk.Value.NegZNeighbor) && this.ChunkExists(chunk.Value.PosZNeighbor))
                    {
                        chunk.Value.Update();
                    }
                }
                Debug.Log($@"{GameManager.time}: Finished Starter Zone Meshing Initialization");
                Logger.Log($@"{GameManager.time}: Finished Starter Zone Meshing Initialization");
                // Generate GameObject and MeshRenderer for each of the loaded Chunks
                foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                {
                    chunk.Value.OnUnityUpdate();
                }
                this.starterChunksInitialized = true;
                Debug.Log($@"{GameManager.time}: Finished Starter Zone Rendering Initialization");
                Logger.Log($@"{GameManager.time}: Finished Starter Zone Rendering Initialization");
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
            // After starting zone is initialized, start a thread to continuously update chunks around player
            Thread worldThread = new Thread(() =>
            {
                while(this.IsRunning)
                {
                    try
                    {
                        // If starting zone is initialized and player doesn't already exist, try instantiating player
                        if(this.starterChunksInitialized && !GameManager.PlayerLoaded())
                        {
                            Int3 PlayerStartPos = MathHelper.GetPlayerStartPosition(this.WorldStartPos);
                            Int3 PlayerStartChunkPos = PlayerStartPos;
                            PlayerStartChunkPos.WorldCoordsToChunkCoords();
                            Chunk chunk = this.GetChunk(PlayerStartChunkPos);
                            GameObject go = chunk.GO;
                            GameManager.Instance.StartPlayer(PlayerStartPos.GetVec3(), go);
                        }
                        // After initial chunk generation and player loaded into world, continuously generate Chunks around player and degenerate Chunks too far away
                        if(this.starterChunksInitialized && GameManager.PlayerLoaded())
                        {
                            // Get current player position
                            Int3 currentPlayerPos = new Int3(GameManager.Instance.PlayerPos);
                            // Iterate through Loaded Chunks and Degenerate if they are too far from current player position
                            foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                            {
                                Int3 chunkPos = chunk.Value.Pos;
                                chunkPos.ChunkCoordsToWorldCoords();
                                if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) >= (renderDistance * this.distanceToDegenerateMultiplier * Chunk.ChunkSize))
                                {
                                    if(!this.chunksToRemove.Contains(chunk.Key))
                                    {
                                        chunk.Value.Degenerate();
                                    }
                                }
                            }
                            // Remove degenerated Chunks from loaded Chunks list
                            this.RemoveChunks();
                            // Continuously generate Chunks around player
                            for(int x = -RenderDistanceFirstPass; x < RenderDistanceFirstPass; x++)
                            {
                                for(int y = -RenderDistanceFirstPass; y < RenderDistanceFirstPass; y++)
                                {
                                    for(int z = -RenderDistanceFirstPass; z < RenderDistanceFirstPass; z++)
                                    {
                                        Int3 xyz = new Int3(0, 0, 0);
                                        xyz.SetPos(x, y, z);
                                        Int3 newChunkPos = currentPlayerPos;
                                        newChunkPos += xyz * Chunk.ChunkSize;
                                        newChunkPos.WorldCoordsToChunkCoords();
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
                                            }
                                        }
                                    }
                                }
                            }
                            // Generate blocks for all loaded Chunks
                            foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                            {
                                 taskFactory.StartNew(() => chunk.Value.Start());
                            }
                            // Generate meshes for all loaded Chunks
                            foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                            {
                                Int3 chunkPos = chunk.Value.Pos;
                                chunkPos.ChunkCoordsToWorldCoords();
                                if(Vector3.Distance(chunkPos.GetVec3(), currentPlayerPos.GetVec3()) <= (renderDistance * Chunk.ChunkSize))
                                {
                                    if(this.ChunkExists(chunk.Value.NegXNeighbor) && this.ChunkExists(chunk.Value.PosXNeighbor)
                                    && this.ChunkExists(chunk.Value.NegYNeighbor) && this.ChunkExists(chunk.Value.PosYNeighbor)
                                    && this.ChunkExists(chunk.Value.NegZNeighbor) && this.ChunkExists(chunk.Value.PosZNeighbor))
                                    {
                                        // Before update, if Chunk has been set that its neighbors need to update, tell those neighbors they need to update
                                        // Neighbors will need to update meshes if a block is changed at the intersection of chunks to avoid holes between chunks
                                        if(chunk.Value.NeedToUpdateNegXNeighbor)
                                        {
                                            this.GetChunk(chunk.Value.NegXNeighbor).NeedToUpdate = true;
                                            chunk.Value.NeedToUpdateNegXNeighbor = false;
                                        }
                                        if(chunk.Value.NeedToUpdatePosXNeighbor)
                                        {
                                            this.GetChunk(chunk.Value.PosXNeighbor).NeedToUpdate = true;
                                            chunk.Value.NeedToUpdatePosXNeighbor = false;
                                        }
                                        if(chunk.Value.NeedToUpdateNegYNeighbor)
                                        {
                                            this.GetChunk(chunk.Value.NegYNeighbor).NeedToUpdate = true;
                                            chunk.Value.NeedToUpdateNegYNeighbor = false;
                                        }
                                        if(chunk.Value.NeedToUpdatePosYNeighbor)
                                        {
                                            this.GetChunk(chunk.Value.PosYNeighbor).NeedToUpdate = true;
                                            chunk.Value.NeedToUpdatePosYNeighbor = false;
                                        }
                                        if(chunk.Value.NeedToUpdateNegZNeighbor)
                                        {
                                            this.GetChunk(chunk.Value.NegZNeighbor).NeedToUpdate = true;
                                            chunk.Value.NeedToUpdateNegZNeighbor = false;
                                        }
                                        if(chunk.Value.NeedToUpdatePosZNeighbor)
                                        {
                                            this.GetChunk(chunk.Value.PosZNeighbor).NeedToUpdate = true;
                                            chunk.Value.NeedToUpdateNegZNeighbor = false;
                                        }
                                        taskFactory.StartNew(() => chunk.Value.Update());
                                    }
                                }
                            }
                            foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
                            {
                                GameManager.Instance.RegisterDelegate(new Action(() => chunk.Value.OnUnityUpdate()));
                            }
                        }
                    }
                    catch(System.Exception e)
                    {
                        Debug.Log(e.ToString());
                        Logger.Log(e);
                    }
                }
                Logger.Log($@"{GameManager.time}: World thread stopped.");
                Logger.MainLog.Update();
            });
            worldThread.Start();
        }
    }
    
    // Update is called once per frame
    public void Update()
    {

    }

    // On Application Quit, save Chunks to file and stop world threads
    public void OnApplicationQuit()
    {
        foreach(KeyValuePair<Int3, Chunk> chunk in this.LoadedChunks)
        {
            try
            {
                // Only save chunks which have been modified by player
                if(chunk.Value.HasBeenModified)
                {
                    Serializer.Serialize_ToFile_FullPath(FileManager.GetChunkString(chunk.Value.Pos), chunk.Value.GetChunkSaveData());
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
        //Debug.Log($@"{GameManager.time}: Added Chunk to remove list: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
        //Logger.Log($@"{GameManager.time}: Added Chunk to remove list: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
    }

    // Remove Chunk from World
    private void RemoveChunks()
    {
        if(this.chunksToRemove.Count > 0)
        {
            foreach(Int3 chunkPos in this.chunksToRemove)
            {
                this.LoadedChunks.Remove(chunkPos);
                //Debug.Log($@"{GameManager.time}: Removed Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
                //Logger.Log($@"{GameManager.time}: Removed Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
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
        chunkPos.WorldCoordsToChunkCoords();
        Int3 internalPos = pos;
        internalPos.WorldCoordsToInternalChunkCoords();
        if(this.ChunkExists(chunkPos))
        {
            Block b = this.GetChunk(chunkPos).GetBlockFromChunkInternalCoords(internalPos);
            return b;
        }
        Debug.Log($@"{GameManager.time}: Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z} does not exist.");
        Logger.Log($@"{GameManager.time}: Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z} does not exist.");
        throw new System.Exception("Trying to get Blocks from Chunk that does not exist: ");
    }
}
