using System.Threading.Tasks;

using UnityEngine;


/// <summary>
/// A column of the game world containing chunks.
/// </summary>
public class Column
{
    /// <summary>
    /// The position of this column.
    /// </summary>
    public Vector2Int ColumnPos { get; private set; }
    /// <summary>
    /// Array of all the chunks in this column.
    /// </summary>
    public Chunk[] Chunks { get; private set; } = new Chunk[GameManager.Instance.ChunksPerColumn];

    /// <summary>
    /// Specific Constructor: Creates a new column at the given position.
    /// </summary>
    /// <param name="columnPos">The position of this column.</param>
    public Column(Vector2Int columnPos)
    {
        this.ColumnPos = columnPos;
        for(int y = 0; y < GameManager.Instance.ChunksPerColumn; y++)
        {
            this.Chunks[y] = new Chunk(new Vector3Int(this.ColumnPos.x, y, this.ColumnPos.y));
        }
    }

    /// <summary>
    /// Tries to get the chunk at the given y position from this column.
    /// </summary>
    /// <param name="chunkPosY">The y position to retrieve the chunk from.</param>
    /// <param name="chunk">The chunk output if it exists.</param>
    /// <returns>Returns true if a chunk exists at that location.</returns>
    public bool TryGetChunk(int chunkPosY, out Chunk chunk)
    {
        if(chunkPosY >= 0 && chunkPosY < this.Chunks.Length)
        {
            chunk = this.Chunks[chunkPosY];
            return true;
        }
        else
        {
            chunk = null;
            return false;
        }
    }

    /// <summary>
    /// Generates chunk surface data for all chunks in this column.
    /// </summary>
    public void GenerateChunkSurfaceData()
    {
        Parallel.For(0, GameManager.Instance.ChunksPerColumn, y =>
        {
            if(SaveSystem.TryLoadChunkFromDrive(new Vector3Int(this.ColumnPos.x, y, this.ColumnPos.y), out Chunk chunk) == true)
            {
                this.Chunks[y] = chunk;
            }
            if(this.Chunks[y].HasGeneratedChunkData == false)
            {
                this.Chunks[y].GenerateChunkSurfaceData();
            }
        });
    }

    /// <summary>
    /// Generates chunk block data for all chunks in this column.
    /// </summary>
    public void GenerateChunkBlockData()
    {
        Vector3Int columnMinPos = new Vector3Int(this.ColumnPos.x, 0, this.ColumnPos.y).ChunkPosToWorldPos();
        Parallel.For(columnMinPos.x, columnMinPos.x + GameManager.Instance.ChunkSize, x =>
        {
            Parallel.For(columnMinPos.z, columnMinPos.z + GameManager.Instance.ChunkSize, z =>
            {
                int closestAir = GameManager.Instance.ChunkSize * GameManager.Instance.ChunksPerColumn;
                int dirtDepth = Mathf.RoundToInt(GameManager.Instance.NoiseGeneratorBase.GetNoise(x, z).Remap(-1, 1, 2, 6));
                for(int y = closestAir - 1; y >= 0; y--)
                {
                    Vector3Int worldPos = new Vector3Int(x, y, z);
                    Vector3Int internalPos = worldPos.WorldPosToInternalPos();
                    if(this.TryGetChunk(worldPos.WorldPosToChunkPos().y, out Chunk chunk) == true)
                    {
                        if(chunk.HasGeneratedChunkData == false)
                        {
                            if(chunk.GetSurfaceData(internalPos) == 1)
                            {
                                if(y == 0)
                                {
                                    chunk.SetBlock(internalPos, Block.Bedrock);
                                }
                                else
                                {
                                    if(y >= GameManager.Instance.ChunkSize * GameManager.Instance.ChunksPerColumn / 2)
                                    {
                                        if(closestAir - y <= 1)
                                        {
                                            chunk.SetBlock(internalPos, Block.Grass);
                                        }
                                        else if(closestAir - y <= dirtDepth)
                                        {
                                            chunk.SetBlock(internalPos, Block.Dirt);
                                        }
                                        else
                                        {
                                            chunk.SetBlock(internalPos, Block.Stone);
                                        }
                                    }
                                    else
                                    {
                                        chunk.SetBlock(internalPos, Block.Stone);
                                    }
                                }
                            }
                            else
                            {
                                closestAir = y;
                                chunk.SetBlock(internalPos, Block.Air);
                            }
                        }
                    }
                }
            });
        });
        Parallel.For(0, GameManager.Instance.ChunksPerColumn, y =>
        {
            if(this.Chunks[y].HasGeneratedChunkData == false)
            {
                this.Chunks[y].GenerateChunkBlockData();
            }
        });
    }

    /// <summary>
    /// Generates mesh data for all chunks in this column.
    /// </summary>
    public void GenerateMeshData()
    {
        Parallel.For(0, GameManager.Instance.ChunksPerColumn, y =>
        {
            if(this.Chunks[y].HasGeneratedMeshData == false)
            {
                this.Chunks[y].GenerateMeshData();
            }
        });
    }

    /// <summary>
    /// Generates game objects for all chunks in this column.
    /// </summary>
    public void GenerateChunkGameObject()
    {
        for(int y = 0; y < GameManager.Instance.ChunksPerColumn; y++)
        {
            if(this.Chunks[y].HasGeneratedGameObject == false)
            {
                this.Chunks[y].GenerateChunkGameObject();
            }
        }
    }

    /// <summary>
    /// Degenerates all chunks in this column.
    /// </summary>
    public void Degenerate()
    {
        for(int y = 0; y < GameManager.Instance.ChunksPerColumn; y++)
        {
            this.Chunks[y].Degenerate();
        }
    }
}
