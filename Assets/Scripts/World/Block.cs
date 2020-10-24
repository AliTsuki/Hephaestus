using System;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Struct describing a block of the game world.
/// </summary>
public struct Block
{
    // TODO: Add more block types.
    // TODO: Add support for liquid blocks.
    // Start of Block List
    public static Dictionary<int, Block> BlockTypes { get; private set; } = new Dictionary<int, Block>();
    public static Block Air = new Block("Air", 1, TransparencyEnum.Transparent, 0, 0, 0, (RandomFacesEnum)0b000000);
    public static Block Grass = new Block("Grass", 2, TransparencyEnum.Opaque, 0, 0, UniqueFacesEnum.Bottom | UniqueFacesEnum.Front | UniqueFacesEnum.Back | UniqueFacesEnum.Right | UniqueFacesEnum.Left, (RandomFacesEnum)0b000011, bottomTextureName: "2", frontTextureName: "1", backTextureName: "1", rightTextureName: "1", leftTextureName: "1");
    public static Block Dirt = new Block("Dirt", 3, TransparencyEnum.Opaque, 0, 0, 0, (RandomFacesEnum)0b111111);
    public static Block Stone = new Block("Stone", 4, TransparencyEnum.Opaque, 0, 0, 0, (RandomFacesEnum)0b111111);
    public static Block Bedrock = new Block("Bedrock", 5, TransparencyEnum.Opaque, 0, 0, 0, (RandomFacesEnum)0b111111);
    // End of Block List


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
            this.Block = BlockTypes[saveData.ID];
        }
    }

    #region Block Data
    /// <summary>
    /// The name of this block type.
    /// </summary>
    public string BlockName { get; private set; }
    /// <summary>
    /// The ID value for this block type.
    /// </summary>
    public int ID { get; private set; }
    /// <summary>
    /// Which of this block's faces textures should be different from the main texture for this block.
    /// </summary>
    public UniqueFacesEnum UniqueFaces { get; private set; }
    /// <summary>
    /// Which of this block's faces can have their UVs randomly rotated.
    /// </summary>
    public RandomFacesEnum RandomFaces { get; private set; }
    /// <summary>
    /// The transparency of this block type.
    /// </summary>
    public TransparencyEnum Transparency { get; private set; }
    /// <summary>
    /// The amount of light this block emits.
    /// </summary>
    public int LightEmissionValue { get; private set; }
    /// <summary>
    /// The amount of light this block receives.
    /// </summary>
    public int LightValue { get; private set; }
    /// <summary>
    /// The suffix for this block's unique top face texture.
    /// </summary>
    public string TopTextureName { get; private set; }
    /// <summary>
    /// The suffix for this block's unique bottom face texture.
    /// </summary>
    public string BottomTextureName { get; private set; }
    /// <summary>
    /// The suffix for this block's unique front face texture.
    /// </summary>
    public string FrontTextureName { get; private set; }
    /// <summary>
    /// The suffix for this block's unique back face texture.
    /// </summary>
    public string BackTextureName { get; private set; }
    /// <summary>
    /// The suffix for this block's unique right face texture.
    /// </summary>
    public string RightTextureName { get; private set; }
    /// <summary>
    /// The suffix for this block's unique left face texture.
    /// </summary>
    public string LeftTextureName { get; private set; }
    /// <summary>
    /// Random number assigned to this block.
    /// </summary>
    public int RandomNumber { get; private set; }
    #endregion Block Data

    /// <summary>
    /// How transparent this block is.
    /// </summary>
    public enum TransparencyEnum
    {
        Transparent,
        SemiTransparent,
        Opaque,
    }

    [Flags]
    public enum UniqueFacesEnum
    {
        None = 0,
        Top = 1<<0,
        Bottom = 1<<1,
        Front = 1<<2,
        Back = 1<<3,
        Left = 1<<4,
        Right = 1<<5
    }

    [Flags]
    public enum RandomFacesEnum
    {
        None = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Front = 1 << 2,
        Back = 1 << 3,
        Left = 1 << 4,
        Right = 1 << 5
    }


    /// <summary>
    /// Specific Constructor: Creates a new block with the given block name and transparency.
    /// </summary>
    /// <param name="blockName">The name to give this block.</param>
    /// <param name="transparency">The transparency value to give this block.</param>
    public Block(string blockName, int id, TransparencyEnum transparency, int lightEmissionValue, int lightValue, UniqueFacesEnum uniqueFaces, RandomFacesEnum randomFaces, string topTextureName = "", string bottomTextureName = "", string frontTextureName = "", string backTextureName = "", string rightTextureName = "", string leftTextureName = "")
    {
        this.BlockName = blockName;
        this.ID = id;
        this.Transparency = transparency;
        this.LightEmissionValue = lightEmissionValue;
        this.LightValue = lightValue;
        this.UniqueFaces = uniqueFaces;
        this.RandomFaces = randomFaces;
        this.TopTextureName = topTextureName;
        this.BottomTextureName = bottomTextureName;
        this.FrontTextureName = frontTextureName;
        this.BackTextureName = backTextureName;
        this.RightTextureName = rightTextureName;
        this.LeftTextureName = leftTextureName;
        this.RandomNumber = World.random.Next(0, 4);
        BlockTypes.Add(this.ID, this);
    }

    /// <summary>
    /// Specific Constructor: Creates a new block from the save data read from file.
    /// </summary>
    /// <param name="saveData">The save data to use in creating this block.</param>
    public Block(SaveDataObjects.BlockSaveData saveData)
    {
        this.BlockName = BlockTypes[saveData.ID].BlockName;
        this.ID = saveData.ID;
        this.Transparency = BlockTypes[saveData.ID].Transparency;
        this.LightEmissionValue = BlockTypes[saveData.ID].LightEmissionValue;
        this.LightValue = BlockTypes[saveData.ID].LightValue;
        this.UniqueFaces = BlockTypes[saveData.ID].UniqueFaces;
        this.RandomFaces = BlockTypes[saveData.ID].RandomFaces;
        this.TopTextureName = BlockTypes[saveData.ID].TopTextureName;
        this.BottomTextureName = BlockTypes[saveData.ID].BottomTextureName;
        this.FrontTextureName = BlockTypes[saveData.ID].FrontTextureName;
        this.BackTextureName = BlockTypes[saveData.ID].BackTextureName;
        this.RightTextureName = BlockTypes[saveData.ID].RightTextureName;
        this.LeftTextureName = BlockTypes[saveData.ID].LeftTextureName;
        this.RandomNumber = World.random.Next(0, 4);
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
