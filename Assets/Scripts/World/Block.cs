using UnityEngine;


/// <summary>
/// Struct describing a block of the game world.
/// </summary>
public struct Block
{
    /// <summary>
    /// Struct containing information to update a block.
    /// </summary>
    public struct BlockUpdate
    {
        /// <summary>
        /// The world position of this block update.
        /// </summary>
        public Vector3Int WorldPos { get; private set; }
        /// <summary>
        /// The block to set.
        /// </summary>
        public Block Block { get; private set; }

        /// <summary>
        /// Specific Constructor: Creates a block update parameter struct to use to send block updates.
        /// </summary>
        /// <param name="worldPos">The world position of this block update.</param>
        /// <param name="block">The block are we setting at the given position.</param>
        public BlockUpdate(Vector3Int worldPos, Block block)
        {
            this.WorldPos = worldPos;
            this.Block = block;
        }

        /// <summary>
        /// Specific Constructor: Creates a new block update from the save data read from file.
        /// </summary>
        /// <param name="saveData">The save data to use in creating this block update.</param>
        public BlockUpdate(SaveDataObjects.BlockUpdateSaveData saveData)
        {
            this.WorldPos = saveData.WorldPos.ToVector3Int();
            this.Block = new Block(BlockType.BlockTypes[saveData.ID]);
        }
    }

    #region Block Data
    /// <summary>
    /// The ID value for this block type.
    /// </summary>
    public int ID;
    /// <summary>
    /// The amount of light this block receives.
    /// </summary>
    public int LightValue;
    #endregion Block Data


    /// <summary>
    /// Specific Constructor: Creates a new block of the given type.
    /// </summary>
    /// <param name="type">The type of block to create.</param>
    public Block(BlockType type)
    {
        this.ID = type.ID;
        this.LightValue = 0;
    }

    /// <summary>
    /// Specific Constructor: Creates a new block of the given type.
    /// </summary>
    /// <param name="type">The type of block to create.</param>
    /// <param name="lightValue">The light value of this block.</param>
    public Block(BlockType type, int lightValue)
    {
        this.ID = type.ID;
        this.LightValue = lightValue;
    }

    /// <summary>
    /// Specific Constructor: Creates a new block from the save data read from file.
    /// </summary>
    /// <param name="saveData">The save data to use in creating this block.</param>
    public Block(SaveDataObjects.BlockSaveData saveData)
    {
        this.ID = saveData.ID;
        this.LightValue = 0;
    }

    /// <summary>
    /// Creates mesh data for the given block based on the block type and which of its faces are visible.
    /// </summary>
    /// <param name="internalPos">The position of the block in internal coordinate system.</param>
    /// <param name="chunkPos">The position of the chunk containing this block in chunk coordinate system.</param>
    /// <returns>Returns the created mesh data.</returns>
    public MeshData CreateMesh(Vector3Int internalPos, Vector3Int chunkPos)
    {
        return MeshBuilder.CreateMesh(this, internalPos, chunkPos);
    }
}
