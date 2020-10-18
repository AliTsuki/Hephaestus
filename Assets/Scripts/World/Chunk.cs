using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;


/// <summary>
/// Class describing a chunk of the game world made of blocks.
/// </summary>
public class Chunk
{
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
                        if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Min)
                        {
                            value = Mathf.Min(GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1), GameManager.Instance.NoiseGenerator2.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1)) + (GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.Instance.ChunksPerColumn / 2 * GameManager.Instance.ChunkSize)));
                        }
                        else if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Max)
                        {
                            value = Mathf.Max(GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1), GameManager.Instance.NoiseGenerator2.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1)) + (GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.Instance.ChunksPerColumn / 2 * GameManager.Instance.ChunkSize)));
                        }
                        else if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Average)
                        {
                            value = ((GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1) + GameManager.Instance.NoiseGenerator2.GetNoise(worldPos.x, worldPos.y, worldPos.z)) / 2f).Remap(-1, 1, 0, 1) + (GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.Instance.ChunksPerColumn / 2 * GameManager.Instance.ChunkSize)));
                        }
                        else if(GameManager.Instance.NoiseCombination == GameManager.NoiseCombinationEnum.Just1)
                        {
                            value = GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1) + (GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.Instance.ChunksPerColumn / 2 * GameManager.Instance.ChunkSize)));
                        }
                        else
                        {
                            value = GameManager.Instance.NoiseGenerator2.GetNoise(worldPos.x, worldPos.y, worldPos.z).Remap(-1, 1, 0, 1) + (GameManager.Instance.YMultiplier * (worldPos.y - (GameManager.Instance.ChunksPerColumn / 2 * GameManager.Instance.ChunkSize)));
                        }
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
    /// Generates the block data for this chunk.
    /// </summary>
    public void GenerateChunkBlockData()
    {
        Parallel.For(0, GameManager.Instance.ChunkSize, x =>
        {
            Parallel.For(0, GameManager.Instance.ChunkSize, z =>
            {
                int closestAir = 128;
                for(int y = GameManager.Instance.ChunkSize - 1; y >= 0; y--)
                {
                    // TODO: Add a system to place different kinds of blocks based on noise value.
                    Vector3Int internalPos = new Vector3Int(x, y, z);
                    Vector3Int worldPos = internalPos.InternalPosToWorldPos(this.ChunkPos);
                    if(this.surfaceData[x, y, z] == 1)
                    {
                        if(worldPos.y == 0)
                        {
                            this.SetBlock(internalPos, Block.Bedrock);
                        }
                        else
                        {
                            if(closestAir - y <= 1)
                            {
                                this.SetBlock(internalPos, Block.Grass);
                            }
                            else if(closestAir - y <= 5)
                            {
                                this.SetBlock(internalPos, Block.Dirt);
                            }
                            else
                            {
                                this.SetBlock(internalPos, Block.Stone);
                            }
                        }
                    }
                    else
                    {
                        closestAir = y;
                        this.SetBlock(internalPos, Block.Air);
                    }
                }
            });
        });
        // TODO: Generate Structures & Caves
        // TODO: Update Lighting
        this.HasGeneratedChunkData = true;
        this.UpdateNeighborChunkMeshData();
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
        this.meshRenderer.sharedMaterial = GameManager.Instance.ChunkMaterial;
        this.meshRenderer.sharedMaterial.SetTexture("_BaseColorMap", TextureAtlas.AtlasTexture);
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
    /// Gets the block at the given position in internal coordinate system.
    /// </summary>
    /// <param name="internalPos">The position within the chunk's internal coordinate system to get from.</param>
    /// <returns>Returns the block at that position.</returns>
    public Block GetBlock(Vector3Int internalPos)
    {
        return this.blocks[internalPos.x, internalPos.y, internalPos.z];
    }

    /// <summary>
    /// Sets the given block at the given position.
    /// </summary>
    /// <param name="internalPos">The position to set the block.</param>
    /// <param name="block">The block to place.</param>
    private void SetBlock(Vector3Int internalPos, Block block)
    {
        this.blocks[internalPos.x, internalPos.y, internalPos.z] = block;
    }

    /// <summary>
    /// Sets the block at the given position in internal coordinate system to the given block.
    /// </summary>
    /// <param name="internalPos">The position in internal coordinate system to place the block.</param>
    /// <param name="block">The block to place.</param>
    public void UpdateBlock(Vector3Int internalPos, Block block)
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
