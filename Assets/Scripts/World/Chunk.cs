using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
    /// Array of blocks within this chunk.
    /// </summary>
    public Block[,,] Blocks { get; private set; } = new Block[GameManager.ChunkSize, GameManager.ChunkSize, GameManager.ChunkSize];
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
        { ChunkNeighbors.Top, new Vector3Int( 0,  1,  0)},
        { ChunkNeighbors.Bottom, new Vector3Int( 0, -1,  0)},
        { ChunkNeighbors.Front, new Vector3Int( 0,  0,  1)},
        { ChunkNeighbors.Back, new Vector3Int( 0,  0, -1)},
        { ChunkNeighbors.Right, new Vector3Int( 1,  0,  0)},
        { ChunkNeighbors.Left, new Vector3Int(-1,  0,  0)},
    };

    /// <summary>
    /// Enum of all chunk neighbor positions.
    /// </summary>
    private enum ChunkNeighbors
    {
        Top,
        Bottom,
        Front,
        Back,
        Right,
        Left,
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
            for(int x = 0; x < GameManager.ChunkSize; x++)
            {
                for(int y = 0; y < GameManager.ChunkSize; y++)
                {
                    for(int z = 0; z < GameManager.ChunkSize; z++)
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
    /// Adds a list of block updates sent by neighbor chunks while this chunk was unloaded.
    /// </summary>
    /// <param name="blockUpdates">The list of block updates to apply when this chunk loads.</param>
    public void AddUnloadedChunkBlockUpdates(List<Block.BlockUpdate> blockUpdates)
    {
        foreach(Block.BlockUpdate blockUpdate in blockUpdates)
        {
            this.UnloadedChunkBlockUpdates.Enqueue(blockUpdate);
        }
        if(this.HasGeneratedChunkData == true)
        {
            this.DoUnloadedChunkBlockUpdates();
            this.CalculateLightingData();
            this.UpdateNeighborChunkMeshData();
        }
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
        this.UpdateNeighborChunkMeshData();
        this.HasGeneratedChunkData = true;
    }

    /// <summary>
    /// Generates caves for this chunk.
    /// </summary>
    private void GenerateCaves()
    {
        // TODO: Base number of caves off Y value of chunk so fewer caves generate the higher you go.
        int posOffset = 1000;
        int numWorms = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x, this.ChunkPos.y, this.ChunkPos.z).Remap(-1, 1, GameManager.Instance.MinimumCaveWorms, GameManager.Instance.MaximumCaveWorms));
        for(int i = 0; i < numWorms; i++)
        {
            int posX = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise((this.ChunkPos.x * this.ChunkPos.x) + (posOffset * 1 * i), this.ChunkPos.y + (posOffset * 1 * i), this.ChunkPos.z + (posOffset * 1 * i)).Remap(-1, 1, 0, GameManager.ChunkSize));
            int posY = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 2 * i), (this.ChunkPos.y * this.ChunkPos.y) + (posOffset * 2 * i), this.ChunkPos.z + (posOffset * 2 * i)).Remap(-1, 1, 0, GameManager.ChunkSize));
            int posZ = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 3 * i), this.ChunkPos.y + (posOffset * 3 * i), (this.ChunkPos.z * this.ChunkPos.z) + (posOffset * 3 * i)).Remap(-1, 1, 0, GameManager.ChunkSize));
            Vector3Int newWormPos = new Vector3Int(posX, posY, posZ).InternalPosToWorldPos(this.ChunkPos);
            CaveWorm newWorm = new CaveWorm(newWormPos);
            this.CaveWorms.Add(newWorm);
        }
        Dictionary<Vector3Int, List<Block.BlockUpdate>> chunkUpdates = new Dictionary<Vector3Int, List<Block.BlockUpdate>>();
        foreach(CaveWorm worm in this.CaveWorms)
        {
            foreach(CaveWorm.Segment segment in worm.Segments)
            {
                foreach(Vector3Int point in segment.Points)
                {
                    Vector3Int chunkPos = point.WorldPosToChunkPos();
                    if(chunkUpdates.ContainsKey(chunkPos) == true)
                    {
                        chunkUpdates[chunkPos].Add(new Block.BlockUpdate(point.WorldPosToInternalPos(), new Block(BlockType.Air)));
                    }
                    else
                    {
                        chunkUpdates.Add(chunkPos, new List<Block.BlockUpdate>() { new Block.BlockUpdate(point.WorldPosToInternalPos(), new Block(BlockType.Air)) });
                    }
                }
            }
        }
        foreach(KeyValuePair<Vector3Int, List<Block.BlockUpdate>> chunkUpdate in chunkUpdates)
        {
            if(World.TryGetChunk(chunkUpdate.Key, out Chunk chunk) == true && chunk.HasGeneratedChunkData == true)
            {
                chunk.SetBlock(chunkUpdate.Value);
            }
            else
            {
                World.AddUnloadedChunkBlockUpdates(chunkUpdate.Key, chunkUpdate.Value);
            }
        }
    }

    /// <summary>
    /// Generates ores for this chunk.
    /// </summary>
    private void GenerateOres()
    {
        // TODO: Generate ores...
        // Check ore noise map at chunk pos, remap value to specific min and max for each ore type (base min/max off chunk Y value), use that value as number of veins to produce in this chunk,
        // Loop through amount of veins and check ore position noise map three times, remap values to within chunk bounds, use those values to set center point of each vein,
        // Run ore gen method for each vein, recursively check position on each side (Z+, Z-, Y+....) and check a noise map at each one to determine if point should be added to vein,
        // Every time a point is added subtract from total cost of ore vein, once cost reaches 0 stop generating ore, once all points are determined run set block method with
        // ore updates or addunloadedchunkblockupdates for neighbor chunks yet to generate.
        
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
                this.ApplyBlockUpdate(blockUpdate);
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
        // TODO: Optimize chunk GenerateMeshData, consider compute shader or find a way to multi-thread it
        if(this.HasGeneratedChunkData == true)
        {
            ConcurrentQueue<MeshData> meshDatas = new ConcurrentQueue<MeshData>();
            if(this.meshDataMutex.WaitOne())
            {
                this.ClearMeshData();
                for(int x = 0; x < GameManager.ChunkSize; x++)
                {
                    for(int y = 0; y < GameManager.ChunkSize; y++)
                    {
                        for(int z = 0; z < GameManager.ChunkSize; z++)
                        {
                            Vector3Int internalPos = new Vector3Int(x, y, z);
                            Block block = this.GetBlock(internalPos);
                            if(BlockType.BlockTypes[block.ID].Transparency == BlockType.TransparencyEnum.Opaque)
                            {
                                this.meshData.Merge(block.CreateMesh(internalPos, this.ChunkPos));
                            }
                        }
                    }
                }
                this.HasGeneratedMeshData = true;
            }
            this.meshDataMutex.ReleaseMutex();
            if(this.HasGeneratedGameObject == true)
            {
                World.AddActionToMainThreadQueue(this.AssignMesh);
            }
            else
            {
                World.AddActionToMainThreadQueue(this.GenerateChunkGameObject);
            }
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
        if(this.HasGeneratedGameObject == true)
        {
            ObjectPooler.Destroy(this.ChunkGO);
            this.Blocks = null;
            this.CaveWorms = null;
            this.UnloadedChunkBlockUpdates = null;
            this.HasGeneratedChunkData = false;
            this.HasGeneratedMeshData = false;
            this.HasGeneratedGameObject = false;
            this.HasAssignedMesh = false;
        }
    }

    /// <summary>
    /// Gets surface data for this chunk.
    /// </summary>
    public int GetSurfaceData(Vector3Int internalPos)
    {
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
            value += GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.ChunksPerColumn / 2 * GameManager.ChunkSize));
        }
        if(value >= GameManager.Instance.CutoffValue)
        {
            return 0;
        }
        else
        {
            return 1;
        }
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
    /// Sets the block at the given position in internal coordinate system to the given block.
    /// </summary>
    /// <param name="internalPos">The position in internal coordinate system to set the block.</param>
    /// <param name="block">The block to place.</param>
    public void SetBlock(Vector3Int internalPos, Block block)
    {
        if((this.ChunkPos.y == 0 && internalPos.y == 0) == false)
        {
            this.Blocks[internalPos.x, internalPos.y, internalPos.z] = block;
        }
        else
        {
            if(BlockType.Bedrock.ID == block.ID)
            {
                this.Blocks[internalPos.x, internalPos.y, internalPos.z] = block;
            }
        }
    }

    /// <summary>
    /// Sets multiple blocks at once.
    /// </summary>
    /// <param name="blockUpdates">The list of block updates to apply to this chunk.</param>
    public void SetBlock(List<Block.BlockUpdate> blockUpdates)
    {
        foreach(Block.BlockUpdate blockUpdate in blockUpdates)
        {
            if(blockUpdate.WorldPos.y > 0)
            {
                Vector3Int internalPos = blockUpdate.WorldPos.WorldPosToInternalPos();
                this.SetBlock(internalPos, blockUpdate.Block);
            }
        }
        if(this.HasGeneratedMeshData == true)
        {
            this.GenerateMeshData();
        }
    }

    /// <summary>
    /// Sets the block using the given block update parameters.
    /// </summary>
    /// <param name="blockUpdate">The block update parameters containing a world position and a block type.</param>
    private void ApplyBlockUpdate(Block.BlockUpdate blockUpdate)
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
        this.SetBlock(internalPos, block);
        if(this.HasGeneratedMeshData == true)
        {
            this.GenerateMeshData();
        }
        if(internalPos.x == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.Left);
        }
        else if(internalPos.x == GameManager.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.Right);
        }
        if(internalPos.y == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.Bottom);
        }
        else if(internalPos.y == GameManager.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.Top);
        }
        if(internalPos.z == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.Back);
        }
        else if(internalPos.z == GameManager.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(ChunkNeighbors.Front);
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
            if(World.TryGetChunk(this.ChunkPos + chunkNeighborPosition.Value, out Chunk chunk) && chunk.HasGeneratedChunkData == true && chunk.HasGeneratedMeshData == true)
            {
                chunk.GenerateMeshData();
            }
        }
    }

    /// <summary>
    /// Tells a specific neighbor chunk to update its mesh data due to changes on the border of this chunk and that neighbor.
    /// </summary>
    /// <param name="neighbor">Which neighbor should be updated.</param>
    private void UpdateSpecificNeighborChunkMeshData(ChunkNeighbors neighbor)
    {
        if(World.TryGetChunk(this.ChunkPos + chunkNeighborPositions[neighbor], out Chunk chunk) && chunk.HasGeneratedChunkData == true && chunk.HasGeneratedMeshData == true)
        {
            chunk.GenerateMeshData();
        }
    }
}
