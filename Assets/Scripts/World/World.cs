using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    /// Queue of actions for the main thread to execute. Used to send actions from world thread to main thread for actions that can only be executed on main thread.
    /// </summary>
    private static readonly ConcurrentQueue<Action> mainThreadActionQueue = new ConcurrentQueue<Action>();
    /// <summary>
    /// Queue of block update actions for the world thread to execute. Used to send block updates from main thread to world thread.
    /// </summary>
    private static readonly ConcurrentQueue<Block.BlockUpdateParameters> blockUpdateQueue = new ConcurrentQueue<Block.BlockUpdateParameters>();
    /// <summary>
    /// Dictionary of new chunks to add to world.
    /// </summary>
    private static readonly Dictionary<Vector3Int, Chunk> newChunkGenerationList = new Dictionary<Vector3Int, Chunk>();
    /// <summary>
    /// Dictionary of every chunk that exists in the game world. Indexed by the chunk position in chunk coordinate system.
    /// </summary>
    private static readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    /// <summary>
    /// Position to place the player at the start of the game.
    /// </summary>
    private static Vector3Int playerStartPos = new Vector3Int();
    /// <summary>
    /// The current chunk position of the chunk that contains the player.
    /// </summary>
    private static Vector3Int playerCurrentChunkPos = new Vector3Int();

    /// <summary>
    /// Has the world been generated yet?
    /// </summary>
    public static bool HasGenerated { get; private set; } = false;
    /// <summary>
    /// Should the world thread run?
    /// </summary>
    private static bool shouldWorldThreadRun = false;


    /// <summary>
    /// Creates a new thread to run the world on and generates starting area then continuously updates.
    /// </summary>
    public static void WorldThreadStart()
    {
        shouldWorldThreadRun = true;
        worldThread = new Thread(() =>
        {
            GenerateStartingChunks();
            GetPlayerStartPos();
            while(shouldWorldThreadRun == true)
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
    /// Runs continuously on the world thread. Degenerates old chunks, generates new chunks, updates chunks on command from player inputs.
    /// </summary>
    private static void WorldThreadUpdate()
    {
        while(blockUpdateQueue.Count > 0)
        {
            blockUpdateQueue.TryDequeue(out Block.BlockUpdateParameters blockUpdate);
            UpdateBlock(blockUpdate);
        }
        DegenerateDistantChunks();
        GenerateNewChunks();
    }

    /// <summary>
    /// Called from the main thread via GameManager. Executes any actions passed from the world thread.
    /// </summary>
    public static void MainThreadUpdate()
    {
        if(GameManager.Instance.Player != null)
        {
            playerCurrentChunkPos = GameManager.Instance.Player.transform.position.RoundToInt().WorldPosToChunkPos();
        }
        while(mainThreadActionQueue.Count > 0)
        {
            mainThreadActionQueue.TryDequeue(out Action action);
            action.Invoke();
        }
    }

    /// <summary>
    /// Adds an action to the queue for the world thread to run.
    /// </summary>
    /// <param name="action">The action to add.</param>
    public static void AddBlockUpdateToQueue(Block.BlockUpdateParameters blockUpdate)
    {
        blockUpdateQueue.Enqueue(blockUpdate);
    }

    /// <summary>
    /// Adds an action to the queue for the main thread to run.
    /// </summary>
    /// <param name="action">The action to add.</param>
    public static void AddActionToMainThreadQueue(Action action)
    {
        mainThreadActionQueue.Enqueue(action);
    }

    /// <summary>
    /// Stops the world thread.
    /// </summary>
    public static void Quit()
    {
        shouldWorldThreadRun = false;
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
    /// Updates the block of a chunk described by the given block update parameters.
    /// </summary>
    /// <param name="blockUpdate">The parameters of which block to update and what to update it to.</param>
    private static void UpdateBlock(Block.BlockUpdateParameters blockUpdate)
    {
        Vector3Int chunkPos = blockUpdate.WorldPos.WorldPosToChunkPos();
        Vector3Int internalPos = blockUpdate.WorldPos.WorldPosToInternalPos();
        if(TryGetChunk(chunkPos, out Chunk chunk) == true)
        {
            chunk.UpdateBlock(internalPos, blockUpdate.Block);
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
    /// Checks if chunks are too far from player and degenerates them.
    /// </summary>
    private static void DegenerateDistantChunks()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        List<Chunk> chunksToRemove = new List<Chunk>();
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            if(Vector3Int.Distance(chunk.Value.ChunkPos, playerCurrentChunkPos) > GameManager.Instance.ActiveChunkRadius + 2)
            {
                chunksToRemove.Add(chunk.Value);
            }
        }
        foreach(Chunk chunk in chunksToRemove)
        {
            AddActionToMainThreadQueue(chunk.Degenerate);
            chunks.Remove(chunk.ChunkPos);
        }
        stopwatch.Stop();
        long elapsedMS = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        if(chunksToRemove.Count > 0)
        {
            Logger.Log($@"% DEGENERATE: Took {elapsedMS.ToString("N", CultureInfo.InvariantCulture)} ms to remove {chunksToRemove.Count} distant chunks at a rate of {elapsedMS / (long)chunksToRemove.Count} ms per chunk!");
        }
        chunksToRemove.Clear();
    }

    /// <summary>
    /// Generates new chunks around player.
    /// </summary>
    private static void GenerateNewChunks()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        int radius = GameManager.Instance.ActiveChunkRadius;
        // Create list of new chunks to add
        if(newChunkGenerationList.Count > GameManager.Instance.MaxChunksToQueueForGeneration)
        {
            newChunkGenerationList.Clear();
        }
        for(int x = playerCurrentChunkPos.x - radius + 1; x < playerCurrentChunkPos.x + radius; x++)
        {
            for(int y = playerCurrentChunkPos.y - radius + 1; y < playerCurrentChunkPos.y + radius; y++)
            {
                for(int z = playerCurrentChunkPos.z - radius + 1; z < playerCurrentChunkPos.z + radius; z++)
                {
                    Vector3Int newChunkPos = new Vector3Int(x, y, z);
                    if(TryGetChunk(newChunkPos, out Chunk chunk) == false && newChunkGenerationList.ContainsKey(newChunkPos) == false)
                    {
                        newChunkGenerationList.Add(newChunkPos, new Chunk(newChunkPos));
                    }
                }
            }
        }
        int newChunksArrayLength = newChunkGenerationList.Count < GameManager.Instance.ChunksToGeneratePerThreadLoop ? newChunkGenerationList.Count : GameManager.Instance.ChunksToGeneratePerThreadLoop;
        Chunk[] newChunks = new Chunk[newChunksArrayLength];
        // Generate chunk data
        for(int i = 0; i < newChunks.Length; i++)
        {
            newChunks[i] = newChunkGenerationList.ElementAt(i).Value;
            chunks.Add(newChunks[i].ChunkPos, newChunks[i]);
            newChunks[i].GenerateChunkData();
        }
        // Generate mesh data
        for(int i = 0; i < newChunks.Length; i++)
        {
            newChunks[i].GenerateMeshData();
            AddActionToMainThreadQueue(newChunks[i].GenerateChunkGameObject);
            newChunkGenerationList.Remove(newChunks[i].ChunkPos);
        }
        stopwatch.Stop();
        long elapsedMS = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        if(newChunks.Length > 0)
        {
            Logger.Log($@"* GENERATE: Took {elapsedMS.ToString("N", CultureInfo.InvariantCulture)} ms to generate {newChunks.Length} new chunks at a rate of {elapsedMS / (long)newChunks.Length} ms per chunk! {newChunkGenerationList.Count} chunks left in queue!");
        }
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
        for(int x = -GameManager.Instance.StartingChunkRadius + 1; x < GameManager.Instance.StartingChunkRadius; x ++)
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
        Parallel.ForEach(chunks, chunk =>
        {
            chunk.Value.GenerateChunkData();
        });
        Logger.Log("Successfully Generated Chunk Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate mesh data.
        Logger.Log("Generating Mesh Data for Starting Chunks...");
        Parallel.ForEach(chunks, chunk =>
        {
            chunk.Value.GenerateMeshData();
        });
        Logger.Log("Successfully Generated Mesh Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and create game objects and assign mesh data.
        Logger.Log("Generating GameObjects for Starting Chunks and assigning Mesh Data...");
        Parallel.ForEach(chunks, chunk =>
        {
            AddActionToMainThreadQueue(chunk.Value.GenerateChunkGameObject);
        });
        Logger.Log("Successfully Generated GameObjects and assigned Mesh Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        stopwatch.Stop();
        long elapsedMS = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        Logger.Log($@"Took {elapsedMS.ToString("N", CultureInfo.InvariantCulture)} ms to generate {chunks.Count} chunks at a rate of {elapsedMS / (long)chunks.Count} ms per chunk!");
        HasGenerated = true;
    }

    /// <summary>
    /// Finds an open space to spawn the player character by checking blocks near origin.
    /// </summary>
    private static void GetPlayerStartPos()
    {
        Logger.Log($@"Trying to find player start position...");
        playerStartPos = new Vector3Int(0, (GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1, 0);
        while(CanPlayerSpawnHere(playerStartPos, out bool reachedOutOfBounds) == false)
        {
            if(reachedOutOfBounds == true)
            {
                System.Random random = new System.Random();
                playerStartPos = new Vector3Int(
                    random.Next((GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1),
                                (GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1,
                    random.Next((GameManager.Instance.ChunkSize * (GameManager.Instance.StartingChunkRadius - 1)) - 1));
            }
            else
            {
                playerStartPos += new Vector3Int(0, -1, 0);
            }
        }
        playerStartPos += new Vector3Int(0, 1, 0);
        Logger.Log($@"Player start position found: {playerStartPos}");
        AddActionToMainThreadQueue(PlacePlayerInWorld);
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
            GameManager.Instance.Player = GameObject.Instantiate(GameManager.Instance.PlayerPrefab, playerStartPos, Quaternion.identity, GameManager.Instance.PlayerParent).GetComponent<Player>();
        }
        else
        {
            GameManager.Instance.Player.transform.position = playerStartPos;
            GameManager.Instance.Player.transform.rotation = Quaternion.identity;
        }
    }
}
