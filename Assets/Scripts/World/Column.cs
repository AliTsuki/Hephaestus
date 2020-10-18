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
    public Vector2Int ColumnPos;

    /// <summary>
    /// Array of all the chunks in this column.
    /// </summary>
    public readonly Chunk[] Chunks = new Chunk[GameManager.Instance.ChunksPerColumn];

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
    /// Generates chunk data for all chunks in this column.
    /// </summary>
    public void GenerateChunkData()
    {
        Parallel.For(0, GameManager.Instance.ChunksPerColumn, y =>
        {
            this.Chunks[y].GenerateChunkData();
        });
    }

    /// <summary>
    /// Generates mesh data for all chunks in this column.
    /// </summary>
    public void GenerateMeshData()
    {
        Parallel.For(0, GameManager.Instance.ChunksPerColumn, y =>
        {
            this.Chunks[y].GenerateMeshData();
        });
    }

    /// <summary>
    /// Generates game objects for all chunks in this column.
    /// </summary>
    public void GenerateChunkGameObject()
    {
        for(int y = 0; y < GameManager.Instance.ChunksPerColumn; y++)
        {
            this.Chunks[y].GenerateChunkGameObject();
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
