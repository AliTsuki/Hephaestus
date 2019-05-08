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
    private readonly Dictionary<Int3, Chunk> loadedChunks = new Dictionary<Int3, Chunk>();
    private readonly List<Int3> chunksToRemove = new List<Int3>();
    // Chunks to render around player, FirstPass is initialization, renderDistance is during regular gameplay
    public static int RenderDistanceFirstPass = 4;
    private static readonly int renderDistance = 3;
    private readonly float distanceToDegenerateMultiplier = 2;
    public static readonly TaskFactory TaskFactory = new TaskFactory();
    public static World Instance { get; private set; }

    // Instantiate World, Register loops, set Random World start position
    public static void Instantiate()
    {
        Instance = new World();
        MainLoopable.Instance.RegisterLoops(Instance);
        System.Random random = new System.Random();
        Instance.WorldStartPos = new Int3(random.Next(-1000, 1000), 62, random.Next(-1000, 1000));
    }

    // Start is called before the first frame update
    public void Start()
    {
        this.IsRunning = true;
        if(this.IsRunning)
        {
            try
            {
                this.InitializeStarterZone();
            }
            catch(Exception e)
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
                        this.UpdateChunks();
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.ToString());
                        Logger.Log(e);
                    }
                }
                Logger.Log($@"{GameManager.Time}: World thread stopped.");
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
        foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
        {
            try
            {
                // Only save chunks which have been modified by player
                if(chunk.Value.HasBeenModified)
                {
                    Serializer.SerializeToFile(FileManager.GetChunkString(chunk.Value.Pos), chunk.Value.GetChunkSaveData());
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
                Logger.Log(e);
            }
        }
        this.IsRunning = false;
        Logger.Log($@"{GameManager.Time}: Stopping world threads...");
    }

    // Initialize Starter Zone Chunks
    private void InitializeStarterZone()
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
                                Chunk chunk = new Chunk(newChunkPos, Serializer.DeserializeFromFile<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                this.loadedChunks.Add(newChunkPos, chunk);
                            }
                            catch(Exception e)
                            {
                                Debug.Log(e.ToString());
                                Logger.Log(e);
                            }
                        }
                        // If no save file for chunk, generate new Chunk and add to World
                        else
                        {
                            Chunk chunk = new Chunk(newChunkPos);
                            this.loadedChunks.Add(newChunkPos, chunk);
                        }
                    }
                }
            }
        }
        // Generate blocks for each of the loaded Chunks
        foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
        {
            chunk.Value.Start();
        }
        Debug.Log($@"{GameManager.Time}: Finished Starter Zone Generation Initialization");
        Logger.Log($@"{GameManager.Time}: Finished Starter Zone Generation Initialization");
        // Generate mesh for each of the loaded Chunks (check if they have all neighbors meshed to avoid broken meshes at Chunk intersections)
        foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
        {
            if(this.ChunkExists(chunk.Value.NegXNeighbor) && this.ChunkExists(chunk.Value.PosXNeighbor)
                && this.ChunkExists(chunk.Value.NegYNeighbor) && this.ChunkExists(chunk.Value.PosYNeighbor)
                && this.ChunkExists(chunk.Value.NegZNeighbor) && this.ChunkExists(chunk.Value.PosZNeighbor))
            {
                chunk.Value.Update();
            }
        }
        Debug.Log($@"{GameManager.Time}: Finished Starter Zone Meshing Initialization");
        Logger.Log($@"{GameManager.Time}: Finished Starter Zone Meshing Initialization");
        // Generate GameObject and MeshRenderer for each of the loaded Chunks
        foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
        {
            chunk.Value.OnUnityUpdate();
        }
        this.starterChunksInitialized = true;
        Debug.Log($@"{GameManager.Time}: Finished Starter Zone Rendering Initialization");
        Logger.Log($@"{GameManager.Time}: Finished Starter Zone Rendering Initialization");
    }

    // Chunk Creation / Generation / Update / Degeneration Loop
    private void UpdateChunks()
    {
        // If starting zone is initialized and player doesn't already exist, try instantiating player
        if(this.starterChunksInitialized && !GameManager.PlayerLoaded())
        {
            Int3 PlayerStartPos = this.GetPlayerStartPosition(this.WorldStartPos);
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
            foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
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
                                    Chunk chunk = new Chunk(newChunkPos, Serializer.DeserializeFromFile<int[,,]>(FileManager.GetChunkString(newChunkPos)));
                                    this.loadedChunks.Add(newChunkPos, chunk);
                                }
                                catch(Exception e)
                                {
                                    Debug.Log(e.ToString());
                                    Logger.Log(e);
                                }
                            }
                            // If no save file for chunk, generate new Chunk and add to World
                            else
                            {
                                Chunk chunk = new Chunk(newChunkPos);
                                this.loadedChunks.Add(newChunkPos, chunk);
                            }
                        }
                    }
                }
            }
            // Generate blocks for all loaded Chunks
            foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
            {
                TaskFactory.StartNew(() => chunk.Value.Start());
            }
            // Generate meshes for all loaded Chunks
            foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
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
                        TaskFactory.StartNew(() => chunk.Value.Update());
                    }
                }
            }
            foreach(KeyValuePair<Int3, Chunk> chunk in this.loadedChunks)
            {
                GameManager.Instance.RegisterDelegate(new Action(() => chunk.Value.OnUnityUpdate()));
            }
        }
    }
    // Remove Chunk from world, given Chunk Position
    public void AddChunkToRemoveList(Int3 _chunkPos)
    {
        this.chunksToRemove.Add(_chunkPos);
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
                this.loadedChunks.Remove(chunkPos);
                //Debug.Log($@"{GameManager.time}: Removed Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
                //Logger.Log($@"{GameManager.time}: Removed Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}");
            }
            this.chunksToRemove.Clear();
        }
    }

    // Check if Chunk currently exists, given Chunk Coords
    public bool ChunkExists(Int3 _chunkPos)
    {
        if(this.loadedChunks.ContainsKey(_chunkPos))
        {
            return true;
        }
        return false;
    }

    // Get Chunk at location, given Chunk Coords
    public Chunk GetChunk(Int3 _chunkPos)
    {
        if(this.loadedChunks.ContainsKey(_chunkPos))
        {
            return this.loadedChunks[_chunkPos];
        }
        return new ErroredChunk(_chunkPos);
    }

    // Get Block, given World Coords
    public Block GetBlockFromWorldCoords(Int3 _pos)
    {
        Int3 chunkPos = _pos;
        chunkPos.WorldCoordsToChunkCoords();
        Int3 internalPos = _pos;
        internalPos.WorldCoordsToInternalChunkCoords();
        if(this.ChunkExists(chunkPos))
        {
            return this.GetChunk(chunkPos).GetBlockFromChunkInternalCoords(internalPos);
        }
        Debug.Log($@"{GameManager.Time}: Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z} does not exist.");
        Logger.Log($@"{GameManager.Time}: Chunk: C_{chunkPos.x}_{chunkPos.y}_{chunkPos.z} does not exist.");
        throw new Exception("Trying to get Blocks from Chunk that does not exist: ");
    }

    // Return highest chunk loaded at that X, Z position by checking chunks above recursively
    private Int3 GetHighestChunk(Int3 _chunkPos)
    {
        Int3 highestChunk = _chunkPos;
        Int3 chunkAbove = new Int3(_chunkPos.x, _chunkPos.y + 1, _chunkPos.z);
        if(Instance.ChunkExists(chunkAbove))
        {
            highestChunk = this.GetHighestChunk(chunkAbove);
        }
        return highestChunk;
    }

    // Get highest clear block position for the player start
    private Int3 GetPlayerStartPosition(Int3 _worldStartPos)
    {
        Int3 playerStartPos = _worldStartPos;
        for(int i = playerStartPos.y + (Chunk.ChunkSize * RenderDistanceFirstPass); i >= playerStartPos.y - (Chunk.ChunkSize * RenderDistanceFirstPass); i--)
        {
            Int3 checkPos = new Int3(_worldStartPos.x, i, _worldStartPos.z);
            Int3 checkPos1 = checkPos;
            checkPos1.AddPos(0, 1, 0);
            Int3 checkPos2 = checkPos;
            checkPos2.AddPos(0, 2, 0);
            Int3 checkPosChunk = checkPos;
            checkPosChunk.WorldCoordsToChunkCoords();
            Int3 checkPos1Chunk = checkPos1;
            checkPos1Chunk.WorldCoordsToChunkCoords();
            Int3 checkPos2Chunk = checkPos2;
            checkPos2Chunk.WorldCoordsToChunkCoords();
            if(Instance.ChunkExists(checkPosChunk) && Instance.ChunkExists(checkPos1Chunk) && Instance.ChunkExists(checkPos2Chunk))
            {
                if(!Instance.GetBlockFromWorldCoords(checkPos).IsTransparent && Instance.GetBlockFromWorldCoords(checkPos1).IsTransparent && Instance.GetBlockFromWorldCoords(checkPos2).IsTransparent)
                {
                    playerStartPos.SetPos(playerStartPos.x, i + 1, playerStartPos.z);
                    return playerStartPos;
                }
            }
        }
        System.Random r = new System.Random();
        playerStartPos = this.GetPlayerStartPosition(new Int3(r.Next(-20, 20) + playerStartPos.x, _worldStartPos.y, r.Next(-20, 20) + playerStartPos.z));
        return playerStartPos;
    }
}
