using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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

    #region World Data
    /// <summary>
    /// Queue of actions for the main thread to execute. Used to send actions from world thread to main thread for actions that can only be executed on main thread.
    /// </summary>
    private static readonly ConcurrentQueue<Action> mainThreadActionQueue = new ConcurrentQueue<Action>();
    /// <summary>
    /// Queue of block update actions for the world thread to execute. Used to send block updates from main thread to world thread.
    /// </summary>
    private static readonly ConcurrentQueue<Block.BlockUpdateParameters> blockUpdateQueue = new ConcurrentQueue<Block.BlockUpdateParameters>();
    /// <summary>
    /// Dictionary of all columns currently loaded in game world.
    /// </summary>
    private static readonly Dictionary<Vector2Int, Column> columns = new Dictionary<Vector2Int, Column>();
    #endregion World Data

    #region Player
    /// <summary>
    /// Position to place the player at the start of the game.
    /// </summary>
    private static Vector3Int playerStartPos = new Vector3Int();
    /// <summary>
    /// The current chunk position of the chunk that contains the player.
    /// </summary>
    private static Vector3Int playerCurrentChunkPos = new Vector3Int();
    /// <summary>
    /// The forward direction of the player.
    /// </summary>
    private static Vector3 playerCurrentForward = new Vector3();
    #endregion Player

    #region Flags
    /// <summary>
    /// Has the world been generated yet?
    /// </summary>
    public static bool HasStartingAreaGenerated = false;
    /// <summary>
    /// Should the world thread run?
    /// </summary>
    private static bool shouldWorldThreadRun = false;
    /// <summary>
    /// Has the player been spawned in the world?
    /// </summary>
    private static bool hasPlayerSpawned = false;
    #endregion Flags


    /// <summary>
    /// Creates a new thread to run the world on and generates starting area then continuously updates.
    /// </summary>
    public static void WorldThreadStart()
    {
        shouldWorldThreadRun = true;
        worldThread = new Thread(() =>
        {
            GenerateStartingColumns();
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
            columns.Clear();
        });
        worldThread.Start();
    }

    /// <summary>
    /// Runs continuously on the world thread. Degenerates old chunks, generates new chunks, updates chunks on command from player inputs.
    /// </summary>
    private static void WorldThreadUpdate()
    {
        if(hasPlayerSpawned == true)
        {
            // Do block updates in block update queue
            while(blockUpdateQueue.Count > 0)
            {
                blockUpdateQueue.TryDequeue(out Block.BlockUpdateParameters blockUpdate);
                UpdateBlock(blockUpdate);
            }
            // Degenerate old chunks and generate new chunks
            DegenerateDistantColumns();
            GenerateNextColumn();
            GenerateNewChunks();
        }
    }

    /// <summary>
    /// Called from the main thread via GameManager. Executes any actions passed from the world thread.
    /// </summary>
    public static void MainThreadUpdate()
    {
        // Update player info
        if(hasPlayerSpawned == true)
        {
            playerCurrentChunkPos = GameManager.Instance.Player.transform.position.RoundToInt().WorldPosToChunkPos();
            playerCurrentForward = GameManager.Instance.Player.transform.forward;
        }
        // Do actions in main thread queue
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
    /// Tries to get a column from the list of columns using the given position.
    /// </summary>
    /// <param name="columnPos">The position to retrieve the column from.</param>
    /// <param name="column">The column output if column exists at given position.</param>
    /// <returns>Returns true if a column exists at the given position.</returns>
    public static bool TryGetColumn(Vector2Int columnPos, out Column column)
    {
        if(columns.ContainsKey(columnPos) == true)
        {
            column = columns[columnPos];
            return true;
        }
        else
        {
            column = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get a chunk from the chunk array of a column using the given position.
    /// </summary>
    /// <param name="chunkPos">The position in chunk coordinate system to retrieve a chunk from.</param>
    /// <param name="chunk">The chunk output if chunk exists at given position.</param>
    /// <returns>Returns true if a chunk exists at the given position.</returns>
    public static bool TryGetChunk(Vector3Int chunkPos, out Chunk chunk)
    {
        if(TryGetColumn(new Vector2Int(chunkPos.x, chunkPos.z), out Column column) == true)
        {
            if(column.TryGetChunk(chunkPos.y, out chunk) == true)
            {
                return true;
            }
            return false;
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
            block = new Block();
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
            chunk.PlaceBlock(internalPos, blockUpdate.Block);
        }
    }

    /// <summary>
    /// Checks if columns are too far from player and degenerates them.
    /// </summary>
    private static void DegenerateDistantColumns()
    {
        List<Column> columnsToRemove = new List<Column>();
        foreach(KeyValuePair<Vector2Int, Column> column in columns)
        {
            if(Vector2Int.Distance(column.Value.ColumnPos, new Vector2Int(playerCurrentChunkPos.x, playerCurrentChunkPos.z)) > GameManager.Instance.StartingColumnRadius + 1)
            {
                columnsToRemove.Add(column.Value);
            }
        }
        foreach(Column column in columnsToRemove)
        {
            AddActionToMainThreadQueue(column.Degenerate);
            columns.Remove(column.ColumnPos);
        }
        if(columnsToRemove.Count > 0)
        {
            Logger.Log($@"% DEGENERATE: Removed {columnsToRemove.Count} distant columns!");
        }
    }

    /// <summary>
    /// Generates new columns around player.
    /// </summary>
    private static void GenerateNextColumn()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Column newColumn = GetNextColumnToGenerate();
        if(newColumn != null)
        {
            columns.Add(newColumn.ColumnPos, newColumn);
            newColumn.GenerateChunkSurfaceData();
            newColumn.GenerateChunkBlockData();
            stopwatch.Stop();
            long elapsedMS = stopwatch.ElapsedMilliseconds;
            Logger.Log($@"* GENERATE: Took {elapsedMS.ToString("N", CultureInfo.InvariantCulture)} ms to generate new column!");
        }
    }

    /// <summary>
    /// Generates new chunks around player.
    /// </summary>
    private static void GenerateNewChunks()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        List<Chunk> chunksToGenerate = new List<Chunk>();
        foreach(KeyValuePair<Vector2Int, Column> column in columns)
        {
            foreach(Chunk chunk in column.Value.Chunks)
            {
                if(Vector3Int.Distance(chunk.ChunkPos, playerCurrentChunkPos) <= GameManager.Instance.StartingColumnRadius + 1 && chunk.HasGeneratedChunkData == true && chunk.HasGeneratedMeshData == false && chunk.HasGeneratedGameObject == false)
                {
                    chunksToGenerate.Add(chunk);
                }
            }
        }
        foreach(Chunk chunk in chunksToGenerate)
        {
            chunk.GenerateMeshData();
            AddActionToMainThreadQueue(chunk.GenerateChunkGameObject);
        }
        stopwatch.Stop();
        long elapsedMS = stopwatch.ElapsedMilliseconds;
        if(chunksToGenerate.Count > 0)
        {
            Logger.Log($@"* GENERATE: Took {elapsedMS.ToString("N", CultureInfo.InvariantCulture)} ms to generate {chunksToGenerate.Count} new chunks at a rate of {elapsedMS / chunksToGenerate.Count} ms per chunk!");
        }
    }

    /// <summary>
    /// Gets the next column that is in front of or near to the player and hasn't generated yet.
    /// </summary>
    /// <returns>Returns the next column eligible for generation.</returns>
    private static Column GetNextColumnToGenerate()
    {
        Vector3Int forward = new Vector3(playerCurrentForward.x, 0, playerCurrentForward.z).normalized.RoundToInt();
        Vector2Int forwardNormal = new Vector2Int(forward.x, forward.z);
        Vector2Int playerCurrentColumnPos = new Vector2Int(playerCurrentChunkPos.x, playerCurrentChunkPos.z);
        int x = 0;
        int y = 0;
        for(int i = 0; i < GameManager.Instance.ActiveColumnRadius * GameManager.Instance.ActiveColumnRadius; ++i)
        {
            Vector2Int newColumnPos = new Vector2Int(x + playerCurrentColumnPos.x, y + playerCurrentColumnPos.y);
            if(newColumnPos.ManhattanDistance(playerCurrentColumnPos) < 2 || Vector2.Dot(forwardNormal, (newColumnPos - playerCurrentColumnPos).Normalize()) > 0.5f)
            {
                if(TryGetColumn(newColumnPos, out _) == false)
                {
                    return new Column(newColumnPos);
                }
            }
            if(Mathf.Abs(x) <= Mathf.Abs(y) && (x != y || x >= 0))
            {
                x += (y >= 0) ? 1 : -1;
            }
            else
            {
                y += (x >= 0) ? -1 : 1;
            }
        }
        return null;
    }

    /// <summary>
    /// Generates an area of starting columns and their respective chunks around world origin.
    /// </summary>
    private static void GenerateStartingColumns()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch individualStopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Logger.Log("Generating Starting Columns...");
        //------------------------------------------------------------------------------------------
        // Instantiate Starting Chunks.
        Logger.Log("Initializing Starting Columns...");
        individualStopwatch.Start();
        for(int x = -GameManager.Instance.StartingColumnRadius + 1; x < GameManager.Instance.StartingColumnRadius; x ++)
        {
            for(int z = -GameManager.Instance.StartingColumnRadius + 1; z < GameManager.Instance.StartingColumnRadius; z++)
            {
                Vector2Int newColumnPos = new Vector2Int(x, z);
                if(Vector2Int.zero.ManhattanDistance(newColumnPos) <= GameManager.Instance.StartingColumnRadius)
                {
                    columns.Add(newColumnPos, new Column(newColumnPos));
                }
            }
        }
        individualStopwatch.Stop();
        Logger.Log($@"Successfully Initialized Starting Columns! Took {individualStopwatch.ElapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture)} ms!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate chunk data.
        Logger.Log("Generating Chunk Data for Starting Columns...");
        individualStopwatch.Restart();
        Parallel.ForEach(columns, column =>
        {
            column.Value.GenerateChunkSurfaceData();
            column.Value.GenerateChunkBlockData();
        });
        individualStopwatch.Stop();
        Logger.Log($@"Successfully Generated Chunk Data for Starting Columns! Took {individualStopwatch.ElapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture)} ms!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate mesh data.
        Logger.Log("Generating Mesh Data and Game Objects for Starting Chunks...");
        individualStopwatch.Restart();
        Parallel.ForEach(columns, column =>
        {
            column.Value.GenerateMeshData();
            AddActionToMainThreadQueue(column.Value.GenerateChunkGameObject);
        });
        individualStopwatch.Stop();
        Logger.Log($@"Successfully Generated GameObjects and assigned Mesh Data for Starting Columns! Took {individualStopwatch.ElapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture)} ms!");
        //------------------------------------------------------------------------------------------
        stopwatch.Stop();
        Logger.Log($@"Took {stopwatch.ElapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture)} ms to generate {columns.Count} columns at a rate of {stopwatch.ElapsedMilliseconds / (long)columns.Count} ms per Column or {stopwatch.ElapsedMilliseconds / ((long)columns.Count * GameManager.Instance.ChunksPerColumn)} ms per Chunk!");
        HasStartingAreaGenerated = true;
    }

    /// <summary>
    /// Finds an open space to spawn the player character by checking blocks near origin.
    /// </summary>
    private static void GetPlayerStartPos()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Logger.Log($@"Trying to find player start position...");
        playerStartPos = new Vector3Int(0, (GameManager.Instance.ChunksPerColumn * GameManager.Instance.ChunkSize) - 1, 0);
        while(CanPlayerSpawnHere(playerStartPos, out bool reachedOutOfBounds) == false)
        {
            if(reachedOutOfBounds == true)
            {
                System.Random random = new System.Random();
                playerStartPos = new Vector3Int(
                    random.Next((GameManager.Instance.ChunkSize * (GameManager.Instance.StartingColumnRadius - 1)) - 1),
                                (GameManager.Instance.ChunkSize * GameManager.Instance.ChunksPerColumn) - 1,
                    random.Next((GameManager.Instance.ChunkSize * (GameManager.Instance.StartingColumnRadius - 1)) - 1));
            }
            else
            {
                playerStartPos += new Vector3Int(0, -1, 0);
            }
        }
        playerStartPos += new Vector3Int(0, 1, 0);
        Logger.Log($@"Took {stopwatch.ElapsedMilliseconds.ToString("N", CultureInfo.InvariantCulture)} ms to find a player start position!");
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
        if(hasPlayerSpawned == false)
        {
            GameManager.Instance.Player = GameObject.Instantiate(GameManager.Instance.PlayerPrefab, playerStartPos, Quaternion.identity, GameManager.Instance.PlayerParent).GetComponent<Player>();
            hasPlayerSpawned = true;
        }
        else
        {
            GameManager.Instance.Player.transform.position = playerStartPos;
            GameManager.Instance.Player.transform.rotation = Quaternion.identity;
        }
    }
}
