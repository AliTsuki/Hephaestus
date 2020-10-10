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
    private static readonly Dictionary<chunkNeighbors, Vector3Int> chunkNeighborPositions = new Dictionary<chunkNeighbors, Vector3Int>()
    {
        { chunkNeighbors.XPos, new Vector3Int( 1,  0,  0)},
        { chunkNeighbors.XNeg, new Vector3Int(-1,  0,  0)},
        { chunkNeighbors.YPos, new Vector3Int( 0,  1,  0)},
        { chunkNeighbors.YNeg, new Vector3Int( 0, -1,  0)},
        { chunkNeighbors.ZPos, new Vector3Int( 0,  0,  1)},
        { chunkNeighbors.ZNeg, new Vector3Int( 0,  0, -1)},
    };

    /// <summary>
    /// Enum of all chunk neighbor positions.
    /// </summary>
    private enum chunkNeighbors
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
    /// Generates the block data for this chunk.
    /// </summary>
    public void GenerateChunkData()
    {
        Parallel.For(0, GameManager.Instance.ChunkSize, x =>
        {
            Parallel.For(0, GameManager.Instance.ChunkSize, y =>
            {
                Parallel.For(0, GameManager.Instance.ChunkSize, z =>
                {
                    Vector3Int internalPos = new Vector3Int(x, y, z);
                    Vector3Int worldPos = internalPos.InternalPosToWorldPos(this.ChunkPos);
                    float value = GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z) * (GameManager.Instance.YMultiplier * worldPos.y);
                    this.SetBlock(internalPos, Block.GetBlockFromValue(value));
                });
            });
        });
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
                        this.meshData.Merge(this.GetBlock(internalPos).CreateMesh(internalPos, this.ChunkPos));
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
        this.meshData.Clear();
    }

    /// <summary>
    /// Generates the GameObject for the chunk, places it in the correct position with the correct parenting, adds necessary components,
    /// and creates a mesh and then assigns that mesh to the object.
    /// </summary>
    public void GenerateChunkGameObject()
    {
        this.ChunkGO = new GameObject($@"Chunk: {this.ChunkPos}")
        {
            layer = GameManager.Instance.LevelGeometryLayerMask
        };
        this.ChunkGO.transform.position = this.ChunkPos.ChunkPosToWorldPos();
        this.ChunkGO.transform.parent = GameManager.Instance.ChunkParentTransform;
        this.meshFilter = this.ChunkGO.AddComponent<MeshFilter>();
        this.meshRenderer = this.ChunkGO.AddComponent<MeshRenderer>();
        this.meshRenderer.material = GameManager.Instance.ChunkMaterial;
        this.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        this.meshCollider = this.ChunkGO.AddComponent<MeshCollider>();
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
        // TODO: Degenerate Chunk, save info if chunk was modified
        GameObject.Destroy(this.ChunkGO);
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
            this.UpdateSpecificNeighborChunkMeshData(chunkNeighbors.XNeg);
        }
        else if(internalPos.x == GameManager.Instance.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(chunkNeighbors.XPos);
        }
        if(internalPos.y == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(chunkNeighbors.YNeg);
        }
        else if(internalPos.y == GameManager.Instance.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(chunkNeighbors.YPos);
        }
        if(internalPos.z == 0)
        {
            this.UpdateSpecificNeighborChunkMeshData(chunkNeighbors.ZNeg);
        }
        else if(internalPos.z == GameManager.Instance.ChunkSize - 1)
        {
            this.UpdateSpecificNeighborChunkMeshData(chunkNeighbors.ZPos);
        }
    }

    /// <summary>
    /// Tells chunks neighboring this one to update their meshes if they have already created meshes. This prevents holes in the terrain from chunks generating
    /// meshes due to incomplete information caused by neighboring chunks not yet having generated chunk data.
    /// </summary>
    private void UpdateNeighborChunkMeshData()
    {
        foreach(KeyValuePair<chunkNeighbors, Vector3Int> chunkNeighborPosition in chunkNeighborPositions)
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
    private void UpdateSpecificNeighborChunkMeshData(chunkNeighbors neighbor)
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
