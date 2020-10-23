using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;


/// <summary>
/// Class describing a chunk of the game world made of blocks.
/// </summary>
public class Chunk
{
    // TODO: Create Separate mesh for water, transparent leaves/grass, and glass that use a different shader each. Water and leaves should have vertex displacement in shader to simulate waves/wind.
    #region Chunk Data
    /// <summary>
    /// The position of this chunk in chunk coordinate system.
    /// </summary>
    public Vector3Int ChunkPos { get; private set; } = new Vector3Int();
    /// <summary>
    /// Array of integers corresponding to blocks, 0 is air, 1 is solid ground.
    /// </summary>
    private readonly int[,,] surfaceData = new int[GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize];
    /// <summary>
    /// Array of blocks within this chunk.
    /// </summary>
    public Block[,,] Blocks { get; private set; } = new Block[GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize];
    /// <summary>
	/// List of all the cave worms that started in this chunk.
	/// </summary>
	public List<CaveWorm> CaveWorms { get; private set; } = new List<CaveWorm>();
    /// <summary>
    /// Queue of all block updates sent by neighbor chunks while this chunk was unloaded.
    /// </summary>
    public ConcurrentQueue<Block.BlockUpdate> UnloadedChunkBlockUpdates { get; private set; } = new ConcurrentQueue<Block.BlockUpdate>();
    #endregion Chunk Data

    #region Game Object Data
    /// <summary>
    /// The GameObject associated with this chunk.
    /// </summary>
    private GameObject ChunkGO;
    /// <summary>
    /// The mesh data for the mesh of this chunk.
    /// </summary>
    private readonly MeshData meshData = new MeshData();
    /// <summary>
    /// Mutex that prevents mesh data from being read while it is being updated by another thread.
    /// </summary>
    private readonly Mutex meshDataMutex = new Mutex();
    /// <summary>
    /// The mesh filter component of this chunk.
    /// </summary>
    private MeshFilter meshFilter;
    /// <summary>
    /// The mesh renderer component of this chunk.
    /// </summary>
    private MeshRenderer meshRenderer;
    /// <summary>
    /// The mesh collider component of this chunk.
    /// </summary>
    private MeshCollider meshCollider;
    #endregion Game Object Data

    #region Flags
    /// <summary>
    /// Has this chunk generated chunk data?
    /// </summary>
    public bool HasGeneratedChunkData { get; private set; } = false;
    /// <summary>
    /// Has this chunk generated mesh data?
    /// </summary>
    public bool HasGeneratedMeshData { get; private set; } = false;
    /// <summary>
    /// Has this chunk generated a Game Object?
    /// </summary>
    public bool HasGeneratedGameObject { get; private set; } = false;
    /// <summary>
    /// Has this chunk assigned mesh data to a game object?
    /// </summary>
    public bool HasAssignedMesh { get; private set; } = false;
    #endregion Flags

    /// <summary>
    /// Array of all neighbor positions for a chunk.
    /// </summary>
    private static readonly Dictionary<ChunkNeighbors, Vector3Int> chunkNeighborPositions = new Dictionary<ChunkNeighbors, Vector3Int>()
    {
        { ChunkNeighbors.XPos, new Vector3Int( 1,  0,  0)},
        { ChunkNeighbors.XNeg, new Vector3Int(-1,  0,  0)},
        { ChunkNeighbors.YPos, new Vector3Int( 0,  1,  0)},
        { ChunkNeighbors.YNeg, new Vector3Int( 0, -1,  0)},
        { ChunkNeighbors.ZPos, new Vector3Int( 0,  0,  1)},
        { ChunkNeighbors.ZNeg, new Vector3Int( 0,  0, -1)},
    };

    /// <summary>
    /// Enum of all chunk neighbor positions.
    /// </summary>
    private enum ChunkNeighbors
    {
        XPos,
        XNeg,
        YPos,
        YNeg,
        ZPos,
        ZNeg
    }


    /// <summary>
    /// Specific Constructor: Creates a new chunk with the given position in chunk coordinate system.
    /// </summary>
    /// <param name="chunkPos">The position of this chunk in chunk coordinate system.</param>
    public Chunk(Vector3Int chunkPos)
    {
        this.ChunkPos = chunkPos;
    }

    /// <summary>
    /// Specific Constructor: Creates a new chunk from the save data read from file.
    /// </summary>
    /// <param name="saveData">The save data to use in creating this chunk.</param>
    public Chunk(SaveDataObjects.ChunkSaveData saveData)
    {
        this.ChunkPos = saveData.ChunkPos.ToVector3Int();
        if(saveData.HasGeneratedChunkData == true)
        {
            SaveDataObjects.BlockSaveData[,,] blockArray = saveData.Blocks.To3DArray();
            for(int x = 0; x < GameManager.Instance.ChunkSize; x++)
            {
                for(int y = 0; y < GameManager.Instance.ChunkSize; y++)
                {
                    for(int z = 0; z < GameManager.Instance.ChunkSize; z++)
                    {
                        this.Blocks[x, y, z] = new Block(blockArray[x, y, z]);
                    }
                }
            }
        }
        if(saveData.HasUnloadedChunkBlockUpdates == true)
        {
            for(int i = 0; i < saveData.UnloadedChunkBlockUpdates.Length; i++)
            {
                this.UnloadedChunkBlockUpdates.Enqueue(new Block.BlockUpdate(saveData.UnloadedChunkBlockUpdates[i]));
            }
        }
        this.HasGeneratedChunkData = saveData.HasGeneratedChunkData;
    }

    /// <summary>
    /// Adds a block update sent by neighbor chunks while this chunk was unloaded.
    /// </summary>
    /// <param name="blockUpdate"></param>
    public void AddUnloadedChunkBlockUpdate(Block.BlockUpdate blockUpdate)
    {
        this.UnloadedChunkBlockUpdates.Enqueue(blockUpdate);
    }

    /// <summary>
    /// Generates surface data for this chunk.
    /// </summary>
    public void GenerateChunkSurfaceData()
    {
        Parallel.For(0, GameManager.Instance.ChunkSize, x =>
        {
            Parallel.For(0, GameManager.Instance.ChunkSize, y =>
            {
                Parallel.For(0, GameManager.Instance.ChunkSize, z =>
                {
                    Vector3Int internalPos = new Vector3Int(x, y, z);
                    Vector3Int worldPos = internalPos.InternalPosToWorldPos(this.ChunkPos);
                    float value;
                    if(worldPos.y == 0)
                    {
                        value = -50;
                    }
                    else
                    {
                        float noise1 = GameManager.Instance.NoiseGeneratorBase.GetNoise(worldPos.x, worldPos.y, worldPos.z);
                        float noise2 = GameManager.Instance.NoiseGeneratorRidged.GetNoise(worldPos.x, worldPos.y, worldPos.z);
                        if(GameManager.Instance.InvertBase == true)
                        {
                            noise1 *= -1;
                        }
                        if(GameManager.Instance.InvertRidged == true)
                        {
                            noise2 *= -1;
                        }
                        noise1.Remap(-1, 1, 0, 1);
                        noise2.Remap(-1, 1, 0, 1);
                        if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Min)
                        {
                            value = Mathf.Min(noise1, noise2);
                        }
                        else if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Max)
                        {
                            value = Mathf.Max(noise1, noise2);
                        }
                        else if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Average)
                        {
                            value = (noise1 + noise2) / 2f;
                        }
                        else if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Just1)
                        {
                            value = noise1;
                        }
                        else
                        {
                            value = noise2;
                        }
                        value += GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.Instance.ChunksPerColumn / 2 * GameManager.Instance.ChunkSize));
                    }
                    if(value >= GameManager.Instance.CutoffValue)
                    {
                        this.surfaceData[x, y, z] = 0;
                    }
                    else
                    {
                        this.surfaceData[x, y, z] = 1;
                    }
                });
            });
        });
    }

    /// <summary>
    /// Generates structures and lighting data for chunk.
    /// </summary>
    public void GenerateChunkBlockData()
    {
        this.GenerateCaves();
        this.GenerateOres();
        this.GenerateStructures();
        this.DoUnloadedChunkBlockUpdates();
        this.CalculateLightingData();
        this.HasGeneratedChunkData = true;
        this.UpdateNeighborChunkMeshData();
    }

    /// <summary>
    /// Generates caves for this chunk.
    /// </summary>
    private void GenerateCaves()
    {
        int posOffset = 1000;
        int numWorms = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x, this.ChunkPos.y, this.ChunkPos.z).Remap(-1, 1, GameManager.Instance.MinimumCaveWorms, GameManager.Instance.MaximumCaveWorms));
        for(int i = 0; i < numWorms; i++)
        {
            int posX = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise((this.ChunkPos.x * this.ChunkPos.x) + (posOffset * 1 * i), this.ChunkPos.y + (posOffset * 1 * i), this.ChunkPos.z + (posOffset * 1 * i)).Remap(-1, 1, 0, GameManager.Instance.ChunkSize));
            int posY = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 2 * i), (this.ChunkPos.y * this.ChunkPos.y) + (posOffset * 2 * i), this.ChunkPos.z + (posOffset * 2 * i)).Remap(-1, 1, 0, GameManager.Instance.ChunkSize));
            int posZ = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 3 * i), this.ChunkPos.y + (posOffset * 3 * i), (this.ChunkPos.z * this.ChunkPos.z) + (posOffset * 3 * i)).Remap(-1, 1, 0, GameManager.Instance.ChunkSize));
            Vector3Int newWormPos = new Vector3Int(posX, posY, posZ).InternalPosToWorldPos(this.ChunkPos);
            CaveWorm newWorm = new CaveWorm(newWormPos);
            this.CaveWorms.Add(newWorm);
        }
        foreach(CaveWorm worm in this.CaveWorms)
        {
            foreach(CaveWorm.Segment segment in worm.Segments)
            {
                foreach(Vector3Int point in segment.Points)
                {
                    if(World.TryGetChunk(point.WorldPosToChunkPos(), out Chunk chunk) == true && chunk.HasGeneratedChunkData == true)
                    {
                        chunk.SetBlock(point.WorldPosToInternalPos(), Block.Air);
                    }
                    else
                    {
                        World.AddUnloadedChunkBlockUpdate(new Block.BlockUpdate(point.WorldPosToInternalPos(), Block.Air));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates ores for this chunk.
    /// </summary>
    private void GenerateOres()
    {
        // TODO: Generate ores...
        // Maybe do something similar to cave worm generation, for each chunk, look at a noise map at the chunk location and remap result between min of that ore
        // and max of that ore that should spawn in that chunk, also take into consideration chunk y value for how likely, then spawn an ore node and recursively
        // set blocks to that ore type with some random chance each extra block added to not add more ore so a vein is created of a size given by the chance.
    }

    /// <summary>
    /// Generates structures for this chunk.
    /// </summary>
    private void GenerateStructures()
    {
        // TODO: Generate structures...
        // Probably do somethign similar to cave worm generation, based on y value, do a noise check and use those values to place a certain number of trees / plants etc
        // based on that noise on the surface of the chunk, do a node system similar to cave worm for large structures like dungeons or villages, have a starting node
        // and neighbor nodes branching out to generate the structure. Maybe look into producing large structures on a world level instead of chunk level to avoid super
        // large structures suddenly appearing when you load the chunk where the center node is located.
    }

    /// <summary>
    /// Does all block updates that were sent to this chunk by neighbor chunks before this chunk was loaded.
    /// </summary>
    private void DoUnloadedChunkBlockUpdates()
    {
        while(this.UnloadedChunkBlockUpdates.Count > 0)
        {
            if(this.UnloadedChunkBlockUpdates.TryDequeue(out Block.BlockUpdate blockUpdate))
            {
                this.SetBlock(blockUpdate);
            }
        }
    }

    /// <summary>
    /// Calculates lighting data for this chunk.
    /// </summary>
    private void CalculateLightingData()
    {
        // TODO: Calculate lighting data...
        // Probably do this on a column basis, have the very top blocks of the top chunk of the column set their light value to 16 if they are air to represent sunlight
        // If a block has 16 lighting then set all blocks below to 16 and all blocks to the sides to 15, go down toward the bottom of the column row by row and recursively
        // calculate lighting for each block, if block has a higher lighting value than what you are trying to set then don't change, if lower then raise to (current value - 1)
        // and continue.
    }

    /// <summary>
    /// Creates a mesh by first clearing any existing mesh data then looping through all blocks in the chunk and creating a mesh from the visible faces.
    /// </summary>
    public void GenerateMeshData()
    {
        if(this.meshDataMutex.WaitOne())
        {
            this.ClearMeshData();
            for(int x = 0; x < GameManager.Instance.ChunkSize; x++)
            {
                for(int y = 0; y < GameManager.Instance.ChunkSize; y++)
                {
                    for(int z = 0; z < GameManager.Instance.ChunkSize; z++)
                    {
                        Vector3Int internalPos = new Vector3Int(x, y, z);
                        Block block = this.GetBlock(internalPos);
                        if(block.Transparency == Block.TransparencyEnum.Opaque)
                        {
                            this.meshData.Merge(block.CreateMesh(internalPos, this.ChunkPos));
                        }
                    }
                }
            }
            this.HasGeneratedMeshData = true;
        }
        this.meshDataMutex.ReleaseMutex();
        if(this.HasGeneratedGameObject)
        {
            World.AddActionToMainThreadQueue(this.AssignMesh);
        }
    }

    /// <summary>
    /// Clears all mesh data for this chunk.
    /// </summary>
    private void ClearMeshData()
    {
        if(this.meshDataMutex.WaitOne())
        {
            this.meshData.Clear();
        }
        this.meshDataMutex.ReleaseMutex();
    }

    /// <summary>
    /// Generates the GameObject for the chunk, places it in the correct position with the correct parenting, adds necessary components,
    /// and creates a mesh and then assigns that mesh to the object.
    /// </summary>
    public void GenerateChunkGameObject()
    {
        this.ChunkGO = ObjectPooler.Instantiate(GameManager.Instance.ChunkPrefab, this.ChunkPos.ChunkPosToWorldPos(), Quaternion.identity, GameManager.Instance.ChunkParentTransform);
        this.ChunkGO.layer = GameManager.Instance.LevelGeometryLayerMask;
        this.meshFilter = this.ChunkGO.GetComponent<MeshFilter>();
        this.meshRenderer = this.ChunkGO.GetComponent<MeshRenderer>();
        this.meshRenderer.material = GameManager.Instance.ChunkMaterial;
        this.meshCollider = this.ChunkGO.GetComponent<MeshCollider>();
        this.HasGeneratedGameObject = true;
        this.AssignMesh();
    }

    /// <summary>
    /// Assigns the array of vertices and triangle indices to a mesh object and assign that mesh object to this GameObject's components.
    /// </summary>
    private void AssignMesh()
    {
        if(this.meshDataMutex.WaitOne())
        {
            Mesh mesh = this.meshData.ToMesh();
            this.meshFilter.mesh = mesh;
            this.meshCollider.sharedMesh = mesh;
            this.HasAssignedMesh = true;
        }
        this.meshDataMutex.ReleaseMutex();
    }

    /// <summary>
    /// Called when the chunk is being destroyed.
    /// </summary>
    public void Degenerate()
    {
        if(this.ChunkGO != null)
        {
            ObjectPooler.Destroy(this.ChunkGO);
            this.HasGeneratedGameObject = false;
            this.HasAssignedMesh = false;
        }
    }

    /// <summary>
    /// Gets the surface data at the given position in internal coordinate system.
    /// </summary>
    /// <param name="internalPos">The position within the chunk's internal coordinate system to get from.</param>
    /// <returns>Returns the surface data at that position.</returns>
    public int GetSurfaceData(Vector3Int internalPos)
    {
        return this.surfaceData[internalPos.x, internalPos.y, internalPos.z];
    }

    /// <summary>
    /// Gets the block at the given position in internal coordinate system.
    /// </summary>
    /// <param name="internalPos">The position within the chunk's internal coordinate system to get from.</param>
    /// <returns>Returns the block at that position.</returns>
    public Block GetBlock(Vector3Int internalPos)
    {
        return this.Blocks[internalPos.x, internalPos.y, internalPos.z];
    }

    /// <summary>
    /// Sets the block at the given position in internal coordinate system to the given block. Used by world generator.
    /// </summary>
    /// <param name="internalPos">The position in internal coordinate system to set the block.</param>
    /// <param name="block">The block to place.</param>
    public void SetBlock(Vector3Int internalPos, Block block)
    {
        if(this.ChunkPos.y == 0 && internalPos.y == 0)
        {
            this.Blocks[internalPos.x, internalPos.y, internalPos.z] = Block.Bedrock;
        }
        else
        {
            this.Blocks[internalPos.x, internalPos.y, internalPos.z] = block;
        }
    }

    /// <summary>
    /// Sets the block using the given block update parameters.
    /// </summary>
    /// <param name="blockUpdate">The block update parameters containing a world position and a block type.</param>
    private void SetBlock(Block.BlockUpdate blockUpdate)
    {
        this.SetBlock(blockUpdate.WorldPos.WorldPosToInternalPos(), blockUpdate.Block);
    }

    /// <summary>
    /// Updates the block at the given position in internal coordinate system to the given block. Used by player.
    /// </summary>
    /// <param name="internalPos">The position in internal coordinate system to place the block.</param>
    /// <param name="block">The block to place.</param>
    public void PlaceBlock(Vector3Int internalPos, Block block)
    {
        this.Blocks[internalPos.x, internalPos.y, internalPos.z] = block;
        this.GenerateMeshData();
        if(internalPos.x == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.XNeg);
        }
        else if(internalPos.x == GameManager.Instance.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.XPos);
        }
        if(internalPos.y == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.YNeg);
        }
        else if(internalPos.y == GameManager.Instance.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.YPos);
        }
        if(internalPos.z == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.ZNeg);
        }
        else if(internalPos.z == GameManager.Instance.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.ZPos);
        }
    }

    /// <summary>
    /// Tells chunks neighboring this one to update their meshes if they have already created meshes. This prevents holes in the terrain from chunks generating
    /// meshes due to incomplete information caused by neighboring chunks not yet having generated chunk data.
    /// </summary>
    private void UpdateNeighborChunkMeshData()
    {
        foreach(KeyValuePair<ChunkNeighbors, Vector3Int> chunkNeighborPosition in chunkNeighborPositions)
        {
            if(World.TryGetChunk(this.ChunkPos + chunkNeighborPosition.Value, out Chunk chunk))
            {
                if(chunk.HasGeneratedMeshData)
                {
                    chunk.GenerateMeshData();
                }
            }
        }
    }

    /// <summary>
    /// Tells a specific neighbor chunk to update its mesh data due to changes on the border of this chunk and that neighbor.
    /// </summary>
    /// <param name="neighbor">Which neighbor should be updated.</param>
    private void UpdateSpecificNeighborChunkMeshData(ChunkNeighbors neighbor)
    {
        if(World.TryGetChunk(this.ChunkPos + chunkNeighborPositions[neighbor], out Chunk chunk))
        {
            if(chunk.HasGeneratedMeshData)
            {
                chunk.GenerateMeshData();
            }
        }
    }
}
