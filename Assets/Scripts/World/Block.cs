using UnityEngine;


/// <summary>
/// Class describing a block of the game world.
/// </summary>
public class Block
{
    // Start of Block List
    public static Block Air = new Block("Air", TransparencyEnum.Transparent);
    public static Block Stone = new Block("Stone", TransparencyEnum.Opaque);
    // End of Block List

    /// <summary>
    /// Struct containing information to update a block.
    /// </summary>
    public struct BlockUpdateParameters
    {
        /// <summary>
        /// The world position of this block update.
        /// </summary>
        public Vector3Int WorldPos;
        /// <summary>
        /// The block to set.
        /// </summary>
        public Block Block;

        /// <summary>
        /// Specific Constructor: Creates a block update parameter struct to use to send block updates.
        /// </summary>
        /// <param name="worldPos">The world position of this block update.</param>
        /// <param name="block">The block are we setting at the given position.</param>
        public BlockUpdateParameters(Vector3Int worldPos, Block block)
        {
            this.WorldPos = worldPos;
            this.Block = block;
        }
    }

    /// <summary>
    /// The name of this block type.
    /// </summary>
    public string BlockName { get; private set; }
    /// <summary>
    /// The transparency of this block type.
    /// </summary>
    public TransparencyEnum Transparency { get; private set; }

    /// <summary>
    /// How transparent this block is.
    /// </summary>
    public enum TransparencyEnum
    {
        Opaque,
        SemiTransparent,
        Transparent
    }


    /// <summary>
    /// Specific Constructor: Creates a new block with the given block name and transparency.
    /// </summary>
    /// <param name="blockName">The name to give this block.</param>
    /// <param name="transparency">The transparency value to give this block.</param>
    public Block(string blockName, TransparencyEnum transparency)
    {
        this.BlockName = blockName;
        this.Transparency = transparency;
    }

    /// <summary>
    /// Gets a block type from the given noise value.
    /// </summary>
    /// <param name="value">The noise value.</param>
    /// <returns>Returns the block type represented by the noise value.</returns>
    public static Block GetBlockFromValue(float value)
    {
        if(value >= GameManager.Instance.CutoffValue)
        {
            return Air;
        }
        else
        {
            return Stone;
        }
    }

    /// <summary>
    /// Creates mesh data for the given block based on the block type and which of its faces are visible.
    /// </summary>
    /// <param name="internalPos">The position of the block in internal coordinate system.</param>
    /// <param name="chunkPos">The position of the chunk containing this block in chunk coordinate system.</param>
    /// <returns>Returns the created mesh data.</returns>
    public MeshData CreateMesh(Vector3Int internalPos, Vector3Int chunkPos)
    {
        // If block is air, don't bother drawing
        if(this.Transparency == TransparencyEnum.Transparent)
        {
            return new MeshData();
        }
        // If block is anything else draw it
        else
        {
            return MeshBuilder.DrawCube(internalPos, chunkPos);
        }
    }
}
