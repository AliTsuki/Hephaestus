using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;


/// <summary>
/// Class describing a chunk of the game world made of blocks.
/// </summary>
public class Chunk
{
    #region Chunk Data
    /// <summary>
    /// The position of this chunk in chunk coordinate system.
    /// </summary>
    public Vector3Int ChunkPos { get; private set; }
    /// <summary>
    /// Array of integers corresponding to blocks, 0 is air, 1 is solid ground.
    /// </summary>
    private readonly int[,,] surfaceData = new int[GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize];
    /// <summary>
    /// Array of blocks within this chunk.
    /// </summary>
    private readonly Block[,,] blocks = new Block[GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize, GameManager.Instance.ChunkSize];
    /// <summary>
	/// List of all the cave worms that started in this chunk.
	/// </summary>
	public List<CaveWorm> CaveWorms { get; private set; } = new List<CaveWorm>();
    #endregion Chunk Data

    #region Game Object Data
    /// <summary>
    /// The GameObject associated with this chunk.
    /// </summary>
    public GameObject ChunkGO { get; private set; }
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
        this.CalculateLightingData();
        this.HasGeneratedChunkData = true;
        this.UpdateNeighborChunkMeshData();
    }

    /// <summary>
    /// Generates caves for this chunk.
    /// </summary>
    public void GenerateCaves()
    {
        // TODO: Generate caves
        int posOffset = 1000;
        int numWorms = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x, this.ChunkPos.y, this.ChunkPos.z).Remap(-1, 1, GameManager.Instance.MinimumCaveWorms, GameManager.Instance.MaximumCaveWorms));
        for(int i = 0; i < numWorms; i++)
        {
            int posX = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise((this.ChunkPos.x * this.ChunkPos.x) + (posOffset * 1 * i), this.ChunkPos.y + (posOffset * 1 * i), this.ChunkPos.z + (posOffset * 1 * i)).Remap(-1, 1, 0, GameManager.Instance.ChunkSize));
            int posY = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 2 * i), (this.ChunkPos.y * this.ChunkPos.y) + (posOffset * 2 * i), this.ChunkPos.z + (posOffset * 2 * i)).Remap(-1, 1, 0, GameManager.Instance.ChunkSize));
            int posZ = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 3 * i), this.ChunkPos.y + (posOffset * 3 * i), (this.ChunkPos.z * this.ChunkPos.z) + (posOffset * 3 * i)).Remap(-1, 1, 0, GameManager.Instance.ChunkSize));
            Vector3Int newWormPos = new Vector3Int(posX, posY, posZ).InternalPosToWorldPos(this.ChunkPos);
            CaveWorm newWorm = new CaveWorm(newWormPos, GameManager.Instance.CaveWormRadius);
            this.CaveWorms.Add(newWorm);
        }
        foreach(CaveWorm worm in this.CaveWorms)
        {
            foreach(CaveWorm.Segment segment in worm.Segments)
            {
                foreach(CaveWorm.Segment.Point point in segment.Points)
                {
                    if(World.TryGetChunk(point.WorldPosition.WorldPosToChunkPos(), out Chunk chunk) == true && chunk.HasGeneratedChunkData == true)
                    {
                        chunk.SetBlock(point.WorldPosition.WorldPosToInternalPos(), Block.Air);
                    }
                    else
                    {
                        /// TODO: Set up abandoned block updates
                        /// When cave/ore/structure gen can't update a block in a nearby chunk because it hasn't been generated,
                        /// create the column/chunk to contain them and keep it in a list to be iterated over when chunk finishes normal generation,
                        /// if that chunk ever gets generated, if it doesn't get generated then save teh abandoned block update list to file to read
                        /// when the chunk does get generated if player heads back in that direction later.
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
        // TODO: Generate ores
    }

    /// <summary>
    /// Generates structures for this chunk.
    /// </summary>
    private void GenerateStructures()
    {
        // TODO: Generate structures
    }

    /// <summary>
    /// Calculates lighting data for this chunk.
    /// </summary>
    private void CalculateLightingData()
    {
        // TODO: Calculate lighting data
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
        // TODO: Degenerate Chunk, save info if chunk was modified, add to list of chunks to be saved to disk and run save on world thread
        if(this.ChunkGO != null)
        {
            ObjectPooler.Destroy(this.ChunkGO);
            this.ClearMeshData();
            this.HasGeneratedMeshData = false;
            this.HasGeneratedGameObject = false;
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
        return this.blocks[internalPos.x, internalPos.y, internalPos.z];
    }

    /// <summary>
    /// Sets the block at the given position in internal coordinate system to the given block. Used by world generator.
    /// </summary>
    /// <param name="internalPos">The position in internal coordinate system to set the block.</param>
    /// <param name="block">The block to place.</param>
    public void SetBlock(Vector3Int internalPos, Block block)
    {
        this.blocks[internalPos.x, internalPos.y, internalPos.z] = block;
    }

    /// <summary>
    /// Updates the block at the given position in internal coordinate system to the given block. Used by player.
    /// </summary>
    /// <param name="internalPos">The position in internal coordinate system to place the block.</param>
    /// <param name="block">The block to place.</param>
    public void PlaceBlock(Vector3Int internalPos, Block block)
    {
        this.blocks[internalPos.x, internalPos.y, internalPos.z] = block;
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
