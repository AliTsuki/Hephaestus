using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using UnityEngine;


/// <summary>
/// Static class containing references to everything that exists in the game world.
/// </summary>
public static class World
{
    /// <summary>
    /// The thread the world is running on.
    /// </summary>
    private static Thread worldThread;
    /// <summary>
    /// Mutex used to control access to the list of actions for the main thread to execute.
    /// </summary>
    private readonly static Mutex mainThreadActionsToDoListMutex = new Mutex();
    /// <summary>
    /// List of actions for the main thread to execute. Used to send actions from world thread to main thread for actions that can only be executed on main thread.
    /// </summary>
    private static readonly List<Action> mainThreadActionsToDo = new List<Action>();

    /// <summary>
    /// Dictionary of every chunk that exists in the game world. Indexed by the chunk position in chunk coordinate system.
    /// </summary>
    private static readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    /// <summary>
    /// Position to place the player at the start of the game.
    /// </summary>
    private static Vector3Int playerStartPosition = new Vector3Int();

    /// <summary>
    /// Has the world been generated yet?
    /// </summary>
    public static bool HasGenerated { get; private set; } = false;
    /// <summary>
    /// Should the world thread run?
    /// </summary>
    private static bool ShouldWorldThreadRun = false;


    /// <summary>
    /// Creates a new thread to run the world on and generates starting area then continuously updates.
    /// </summary>
    public static void WorldThreadStart()
    {
        ShouldWorldThreadRun = true;
        worldThread = new Thread(() =>
        {
            GenerateStartingChunks();
            GetPlayerStartPos();
            while(ShouldWorldThreadRun == true)
            {
                try
                {
                    WorldThreadUpdate();
                }
                catch(Exception e)
                {
                    Logger.Log(e);
                }
            }
        });
        worldThread.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    private static void WorldThreadUpdate()
    {

    }

    /// <summary>
    /// Called from the main thread via GameManager. Executes any actions passed from the world thread.
    /// </summary>
    public static void MainThreadUpdate()
    {
        if(mainThreadActionsToDoListMutex.WaitOne())
        {
            foreach(Action action in mainThreadActionsToDo)
            {
                action.Invoke();
            }
            mainThreadActionsToDo.Clear();
        }
        mainThreadActionsToDoListMutex.ReleaseMutex();
    }

    /// <summary>
    /// Stops the world thread.
    /// </summary>
    public static void Quit()
    {
        ShouldWorldThreadRun = false;
        worldThread.Abort();
    }

    /// <summary>
    /// Stops world thread, removes all chunks, and starts the world server up again to create new chunks.
    /// </summary>
    public static void RestartWorld()
    {
        Quit();
        RemoveAllChunks();
        WorldThreadStart();
    }

    /// <summary>
    /// Get a reference to a currently existing chunk by referencing the chunk position in chunk coordinate system.
    /// </summary>
    /// <param name="chunkPos">The position in chunk coordinate system to retrieve a chunk from.</param>
    /// <param name="chunk">The reference to the chunk at that position, if one exists.</param>
    /// <returns>Returns true if a chunk exists at the given position.</returns>
    public static bool TryGetChunk(Vector3Int chunkPos, out Chunk chunk)
    {
        if(chunks.ContainsKey(chunkPos) == true)
        {
            chunk = chunks[chunkPos];
            return true;
        }
        else
        {
            chunk = null;
            return false;
        }
    }

    /// <summary>
    /// Removes all currently existing chunks by destroying their GameObjects and clearing the chunk dictionary.
    /// </summary>
    private static void RemoveAllChunks()
    {
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            GameObject.Destroy(chunk.Value.ChunkGO);
        }
        chunks.Clear();
    }

    /// <summary>
    /// Generates a cube of chunks of StartingChunkRadius doubled minus 1 and cubed in size.
    /// </summary>
    private static void GenerateStartingChunks()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Logger.Log("Generating Starting Chunks...");
        //------------------------------------------------------------------------------------------
        // Instantiate Starting Chunks.
        Logger.Log("Initializing Starting Chunks...");
        for(int x = -GameManager.Instance.StartingChunkRadius + 1; x < GameManager.Instance.StartingChunkRadius; x++)
        {
            for(int y = -GameManager.Instance.StartingChunkRadius + 1; y < GameManager.Instance.StartingChunkRadius; y++)
            {
                for(int z = -GameManager.Instance.StartingChunkRadius + 1; z < GameManager.Instance.StartingChunkRadius; z++)
                {
                    Vector3Int newChunkPos = new Vector3Int(x, y, z);
                    chunks.Add(newChunkPos, new Chunk(newChunkPos));
                }
            }
        }
        Logger.Log("Successfully Initialized Starting Chunks!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate chunk data.
        Logger.Log("Generating Chunk Data for Starting Chunks...");
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            chunk.Value.GenerateChunkData();
        }
        Logger.Log("Successfully Generated Chunk Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate mesh data.
        Logger.Log("Generating Mesh Data for Starting Chunks...");
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            chunk.Value.GenerateMeshData();
        }
        Logger.Log("Successfully Generated Mesh Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and create game objects and assign mesh data.
        Logger.Log("Generating GameObjects for Starting Chunks and assigning Mesh Data...");
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            if(mainThreadActionsToDoListMutex.WaitOne())
            {
                mainThreadActionsToDo.Add(chunk.Value.GenerateChunkGameObject);
            }
            mainThreadActionsToDoListMutex.ReleaseMutex();
        }
        Logger.Log("Successfully Generated GameObjects and assigned Mesh Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        stopwatch.Stop();
        long elapsedMS = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        Logger.Log($@"Took {elapsedMS.ToString("N", CultureInfo.InvariantCulture)} ms to generate {chunks.Count} chunks at a rate of {elapsedMS / (long)chunks.Count} ms per chunk!");
        HasGenerated = true;
    }

    /// <summary>
    /// Returns the block at the given position in world coordinate system.
    /// </summary>
    /// <param name="worldPos">The position in world coordinate system to grab the block from.</param>
    /// <returns>Returns the block at the given position.</returns>
    public static bool TryGetBlockFromWorldPos(Vector3Int worldPos, out Block block)
    {
        Vector3Int chunkPos = worldPos.WorldPosToChunkPos();
        Vector3Int internalPos = worldPos.WorldPosToInternalPos();
        if(TryGetChunk(chunkPos, out Chunk chunk) == true)
        {
            block = chunk.GetBlock(internalPos);
            return true;
        }
        else
        {
            block = null;
            return false;
        }
    }

    /// <summary>
    /// Finds an open space to spawn the player character by checking blocks near origin.
    /// </summary>
    private static void GetPlayerStartPos()
    {
        Logger.Log($@"Trying to find player start position...");
        playerStartPosition = new Vector3Int(0, (GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1, 0);
        Logger.Log($@"Checking for player start position at {playerStartPosition}");
        while(CanPlayerSpawnHere(playerStartPosition, out bool reachedOutOfBounds) == false)
        {
            Logger.Log($@"Checking for player start position at {playerStartPosition}");
            if(reachedOutOfBounds == true)
            {
                System.Random random = new System.Random();
                playerStartPosition = new Vector3Int(
                    random.Next((GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1),
                                (GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1,
                    random.Next((GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1));
            }
            else
            {
                playerStartPosition += new Vector3Int(0, -1, 0);
            }
        }
        playerStartPosition += new Vector3Int(0, 1, 0);
        Logger.Log($@"Player start position found: {playerStartPosition}");
        if(mainThreadActionsToDoListMutex.WaitOne())
        {
            mainThreadActionsToDo.Add(PlacePlayerInWorld);
        }
        mainThreadActionsToDoListMutex.ReleaseMutex();
    }

    /// <summary>
    /// Checks if the player can spawn at a given world position.
    /// </summary>
    /// <param name="worldPos">The position in world coordinate system to check.</param>
    /// <param name="reachedOutOfBounds">Returns true if checks hit world limit and a new random location must be chosen to check.</param>
    /// <returns></returns>
    private static bool CanPlayerSpawnHere(Vector3Int worldPos, out bool reachedOutOfBounds)
    {
        if(TryGetBlockFromWorldPos(worldPos, out Block block) == true)
        {
            if(block.Transparency == Block.TransparencyEnum.Opaque)
            {
                Vector3Int worldPosPlus1 = worldPos + new Vector3Int(0, 1, 0);
                if(TryGetBlockFromWorldPos(worldPosPlus1, out Block blockPlus1) == true)
                {
                    if(blockPlus1.Transparency == Block.TransparencyEnum.Transparent)
                    {
                        Vector3Int worldPosPlus2 = worldPos + new Vector3Int(0, 2, 0);
                        if(TryGetBlockFromWorldPos(worldPosPlus2, out Block blockPlus2) == true)
                        {
                            if(blockPlus2.Transparency == Block.TransparencyEnum.Transparent)
                            {
                                Logger.Log($@"Chunk {worldPos.WorldPosToChunkPos()}:   Block {worldPos}:{block.BlockName}:{block.Transparency}, Block+1 {worldPosPlus1}:{blockPlus1.BlockName}:{blockPlus1.Transparency}, Block+2 {worldPosPlus2}:{blockPlus2.BlockName}:{blockPlus2.Transparency}");
                                reachedOutOfBounds = false;
                                return true;
                            }
                            reachedOutOfBounds = false;
                            return false;
                        }
                        reachedOutOfBounds = true;
                        return false;
                    }
                    reachedOutOfBounds = false;
                    return false;
                }
                reachedOutOfBounds = true;
                return false;
            }
            reachedOutOfBounds = false;
            return false;
        }
        reachedOutOfBounds = true;
        return false;
    }

    /// <summary>
    /// Places the player character in the world.
    /// </summary>
    private static void PlacePlayerInWorld()
    {
        if(GameManager.Instance.Player == null)
        {
            GameManager.Instance.Player = GameObject.Instantiate(GameManager.Instance.PlayerPrefab, playerStartPosition, Quaternion.identity, GameManager.Instance.PlayerParent).GetComponent<Player>();
        }
        else
        {
            GameManager.Instance.Player.transform.position = playerStartPosition;
            GameManager.Instance.Player.transform.rotation = Quaternion.identity;
        }
    }
}
