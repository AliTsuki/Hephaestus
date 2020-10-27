using System;
using System.Collections.Concurrent;
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

    #region World Data
    /// <summary>
    /// Queue of actions for the main thread to execute. Used to send actions from world thread to main thread for actions that can only be executed on main thread.
    /// </summary>
    private static readonly ConcurrentQueue<Action> mainThreadActionQueue = new ConcurrentQueue<Action>();
    /// <summary>
    /// Queue of block update actions for the world thread to execute. Used to send block updates from main thread to world thread.
    /// </summary>
    private static readonly ConcurrentQueue<Block.BlockUpdate> blockUpdateQueue = new ConcurrentQueue<Block.BlockUpdate>();
    /// <summary>
    /// Dictionary of all columns currently loaded in game world.
    /// </summary>
    private static readonly ConcurrentDictionary<Vector2Int, Column> columns = new ConcurrentDictionary<Vector2Int, Column>();
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
    /// The current column position of the column that contains the player.
    /// </summary>
    private static Vector2Int playerCurrentColumnPos = new Vector2Int();
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
    /// Struct containing world save data.
    /// </summary>
    public struct WorldSaveData
    {
        /// <summary>
        /// The name of this world save.
        /// </summary>
        public string WorldSaveName;
        /// <summary>
        /// The seed used to generate this world save.
        /// </summary>
        public int Seed;

        /// <summary>
        /// Specific Constructor: Creates a new World Save Data object with the given parameters.
        /// </summary>
        /// <param name="worldSaveName">The name of the world save.</param>
        /// <param name="seed">The seed for this world save.</param>
        public WorldSaveData(string worldSaveName, int seed)
        {
            this.WorldSaveName = worldSaveName;
            this.Seed = seed;
        }
    }

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
            Logger.WriteLogToFile();
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
            while(blockUpdateQueue.Count > 0)
            {
                blockUpdateQueue.TryDequeue(out Block.BlockUpdate blockUpdate);
                ApplyBlockUpdate(blockUpdate);
            }
            DegenerateDistantColumn();
            while(blockUpdateQueue.Count > 0)
            {
                blockUpdateQueue.TryDequeue(out Block.BlockUpdate blockUpdate);
                ApplyBlockUpdate(blockUpdate);
            }
            GenerateNewColumn();
            while(blockUpdateQueue.Count > 0)
            {
                blockUpdateQueue.TryDequeue(out Block.BlockUpdate blockUpdate);
                ApplyBlockUpdate(blockUpdate);
            }
            GenerateNewChunk();
            Logger.WriteLogToFile();
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
            playerCurrentColumnPos = playerCurrentChunkPos.RemoveY();
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
    public static void AddBlockUpdateToQueue(Block.BlockUpdate blockUpdate)
    {
        blockUpdateQueue.Enqueue(blockUpdate);
    }

    /// <summary>
    /// Adds an block update to a chunk that hasn't loaded yet to run when the chunk does load later.
    /// </summary>
    /// <param name="blockUpdate">The block update to add to a chunk to be performed later.</param>
    public static void AddUnloadedChunkBlockUpdate(Block.BlockUpdate blockUpdate)
    {
        Vector2Int columnPos = blockUpdate.WorldPos.WorldPosToChunkPos().RemoveY();
        if(TryGetColumn(columnPos, out _) == false)
        {
            columns.TryAdd(columnPos, new Column(columnPos));
        }
        if(TryGetChunk(blockUpdate.WorldPos.WorldPosToChunkPos(), out Chunk chunk) == true)
        {
            chunk.AddUnloadedChunkBlockUpdate(blockUpdate);
        }
    }

    /// <summary>
    /// Adds block updates to a an unloaded chunk in bulk.
    /// </summary>
    /// <param name="chunkPos">The chunk position to send block updates to.</param>
    /// <param name="blockUpdates">The list of block updates.</param>
    public static void AddUnloadedChunkBlockUpdateBulk(Vector3Int chunkPos, List<Block.BlockUpdate> blockUpdates)
    {
        Vector2Int columnPos = chunkPos.RemoveY();
        if(TryGetColumn(columnPos, out _) == false)
        {
            columns.TryAdd(columnPos, new Column(columnPos));
        }
        if(TryGetChunk(chunkPos, out Chunk chunk) == true)
        {
            chunk.AddUnloadedChunkBlockUpdateBulk(blockUpdates);
        }
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
        foreach(KeyValuePair<Vector2Int, Column> column in columns)
        {
            column.Value.Degenerate(false);
        }
        columns.Clear();
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
        if(TryGetChunk(chunkPos, out Chunk chunk) == true && chunk.HasGeneratedChunkData == true)
        {
            block = chunk.GetBlock(internalPos);
            return true;
        }
        else
        {
            block = default;
            return false;
        }
    }

    /// <summary>
    /// Updates the block of a chunk described by the given block update parameters.
    /// </summary>
    /// <param name="blockUpdate">The parameters of which block to update and what to update it to.</param>
    private static void ApplyBlockUpdate(Block.BlockUpdate blockUpdate)
    {
        Vector3Int chunkPos = blockUpdate.WorldPos.WorldPosToChunkPos();
        Vector3Int internalPos = blockUpdate.WorldPos.WorldPosToInternalPos();
        if(TryGetChunk(chunkPos, out Chunk chunk) == true && chunk.HasGeneratedChunkData == true)
        {
            chunk.PlaceBlock(internalPos, blockUpdate.Block);
        }
    }

    /// <summary>
    /// Checks if columns are too far from player and degenerates them.
    /// </summary>
    private static void DegenerateDistantColumn()
    {
        Column columnToRemove = null;
        foreach(KeyValuePair<Vector2Int, Column> column in columns)
        {
            if(Vector2Int.Distance(column.Value.ColumnPos, new Vector2Int(playerCurrentChunkPos.x, playerCurrentChunkPos.z)) > GameManager.Instance.StartingColumnRadius + 1)
            {
                columnToRemove = column.Value;
                break;
            }
        }
        if(columnToRemove != null)
        {
            columnToRemove.Degenerate(true);
            columns.TryRemove(columnToRemove.ColumnPos, out _);
        }
    }

    /// <summary>
    /// Generates new columns around player.
    /// </summary>
    private static void GenerateNewColumn()
    {
        Column newColumn = GetNextColumnToGenerate();
        if(newColumn != null)
        {
            columns.TryAdd(newColumn.ColumnPos, newColumn);
            // TODO: Chunk/column generation at edge of world is inconsistent. Starting chunks generate just fine, but new columns/chunks added while exploring don't
            // generate except a handful in the distance, check GenerateNewColumn and GenerateNewChunk to see why.
            newColumn.GenerateChunkBlockData();
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
        int x = 0;
        int z = 0;
        for(int i = 0; i < GameManager.Instance.ActiveColumnRadius * GameManager.Instance.ActiveColumnRadius; ++i)
        {
            Vector2Int newColumnPos = new Vector2Int(x, z) + playerCurrentColumnPos;
            if(newColumnPos.ManhattanDistance(playerCurrentColumnPos) < 2 || Vector2.Dot(forwardNormal, (newColumnPos - playerCurrentColumnPos).Normalize()) > 0.5f)
            {
                if(TryGetColumn(newColumnPos, out _) == false)
                {
                    if(SaveSystem.TryLoadColumnFromDrive(newColumnPos, out Column newColumn) == false)
                    {
                        newColumn = new Column(newColumnPos);
                    }
                    return newColumn;
                }
            }
            if(Mathf.Abs(x) <= Mathf.Abs(z) && (x != z || x >= 0))
            {
                x += (z >= 0) ? 1 : -1;
            }
            else
            {
                z += (x >= 0) ? -1 : 1;
            }
        }
        return null;
    }

    /// <summary>
    /// Generates new chunks around player.
    /// </summary>
    private static void GenerateNewChunk()
    {
        Chunk chunkToGenerate = null;
        for(int x = -GameManager.Instance.ActiveColumnRadius + 1; x < GameManager.Instance.ActiveColumnRadius; x++)
        {
            for(int y = -GameManager.Instance.ActiveColumnRadius + 1; y < GameManager.Instance.ActiveColumnRadius; y++)
            {
                for(int z = -GameManager.Instance.ActiveColumnRadius + 1; z < GameManager.Instance.ActiveColumnRadius; z++)
                {
                    Vector3Int newChunkPos = new Vector3Int(x, y, z) + playerCurrentChunkPos;
                    if(TryGetChunk(newChunkPos, out Chunk chunk) == true && Vector3Int.Distance(newChunkPos, playerCurrentChunkPos) <= GameManager.Instance.ActiveColumnRadius && chunk.HasGeneratedChunkData == true && chunk.HasGeneratedMeshData == false)
                    {
                        chunkToGenerate = chunk;
                        break;
                    }
                }
            }
        }
        if(chunkToGenerate != null)
        {
            chunkToGenerate.GenerateMeshData();
        }
    }

    /// <summary>
    /// Generates an area of starting columns and their respective chunks around world origin.
    /// </summary>
    private static void GenerateStartingColumns()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch individualStopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Restart();
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
                    if(SaveSystem.TryLoadColumnFromDrive(newColumnPos, out Column newColumn) == false)
                    {
                        newColumn = new Column(newColumnPos);
                    }
                    columns.TryAdd(newColumnPos, newColumn);
                }
            }
        }
        individualStopwatch.Stop();
        Logger.Log($@"Successfully Initialized Starting Columns! Took {individualStopwatch.ElapsedMilliseconds.ToString("N0", CultureInfo.InvariantCulture)} ms!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate chunk data.
        Logger.Log("Generating Chunk Data for Starting Columns...");
        individualStopwatch.Restart();
        foreach(KeyValuePair<Vector2Int, Column> column in columns)
        {
            column.Value.GenerateChunkBlockData();
        }
        individualStopwatch.Stop();
        Logger.Log($@"Successfully Generated Chunk Data for Starting Columns! Took {individualStopwatch.ElapsedMilliseconds.ToString("N0", CultureInfo.InvariantCulture)} ms!");
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate mesh data.
        Logger.Log("Generating Mesh Data and Game Objects for Starting Chunks...");
        individualStopwatch.Restart();
        foreach(KeyValuePair<Vector2Int, Column> column in columns)
        {
            column.Value.GenerateMeshData();
        }
        individualStopwatch.Stop();
        Logger.Log($@"Successfully Generated GameObjects and assigned Mesh Data for Starting Columns! Took {individualStopwatch.ElapsedMilliseconds.ToString("N0", CultureInfo.InvariantCulture)} ms!");
        //------------------------------------------------------------------------------------------
        stopwatch.Stop();
        Logger.Log($@"Took {stopwatch.ElapsedMilliseconds.ToString("N0", CultureInfo.InvariantCulture)} ms to generate {columns.Count} columns at a rate of {stopwatch.ElapsedMilliseconds / (long)columns.Count} ms per Column or {stopwatch.ElapsedMilliseconds / ((long)columns.Count * GameManager.Instance.ChunksPerColumn)} ms per Chunk!");
        HasStartingAreaGenerated = true;
    }

    /// <summary>
    /// Finds an open space to spawn the player character by checking blocks near origin.
    /// </summary>
    private static void GetPlayerStartPos()
    {
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
            BlockType blockType = BlockType.BlockTypes[block.ID];
            if(blockType.Transparency == BlockType.TransparencyEnum.Opaque)
            {
                Vector3Int worldPosPlus1 = worldPos + new Vector3Int(0, 1, 0);
                if(TryGetBlockFromWorldPos(worldPosPlus1, out Block blockPlus1) == true)
                {
                    BlockType blockPlus1Type = BlockType.BlockTypes[blockPlus1.ID];
                    if(blockPlus1Type.Transparency == BlockType.TransparencyEnum.Transparent)
                    {
                        Vector3Int worldPosPlus2 = worldPos + new Vector3Int(0, 2, 0);
                        if(TryGetBlockFromWorldPos(worldPosPlus2, out Block blockPlus2) == true)
                        {
                            BlockType blockPlus2Type = BlockType.BlockTypes[blockPlus2.ID];
                            if(blockPlus2Type.Transparency == BlockType.TransparencyEnum.Transparent)
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
