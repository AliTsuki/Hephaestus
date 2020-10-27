using System;
using System.Collections.Generic;


/// <summary>
/// Contains definitions for different types of blocks.
/// </summary>
public class BlockType
{
    // TODO: Add more block types.
    // TODO: Add support for liquid blocks.
    /// <summary>
    /// Contains a list of all the block types available.
    /// </summary>
    public static Dictionary<int, BlockType> BlockTypes { get; private set; } = new Dictionary<int, BlockType>();

    #region Block List
    public static BlockType ERROR { get; private set; } = new BlockType("ERROR", 0, TransparencyEnum.Opaque, 0, (UniqueFacesEnum)0b000000, (RandomFacesEnum)0b000000);
    public static BlockType Air { get; private set; } = new BlockType("Air", 1, TransparencyEnum.Transparent, 0, (UniqueFacesEnum)0b000000, (RandomFacesEnum)0b000000);
    public static BlockType Grass { get; private set; } = new BlockType("Grass", 2, TransparencyEnum.Opaque, 0, (UniqueFacesEnum)0b111110, (RandomFacesEnum)0b000011, bottomTextureName: "2", frontTextureName: "1", backTextureName: "1", rightTextureName: "1", leftTextureName: "1");
    public static BlockType Dirt { get; private set; } = new BlockType("Dirt", 3, TransparencyEnum.Opaque, 0, (UniqueFacesEnum)0b000000, (RandomFacesEnum)0b111111);
    public static BlockType Stone { get; private set; } = new BlockType("Stone", 4, TransparencyEnum.Opaque, 0, (UniqueFacesEnum)0b000000, (RandomFacesEnum)0b111111);
    public static BlockType Bedrock { get; private set; } = new BlockType("Bedrock", 5, TransparencyEnum.Opaque, 0, (UniqueFacesEnum)0b000000, (RandomFacesEnum)0b111111);
    #endregion Block List

    #region Block Data
    /// <summary>
    /// The name of this block type.
    /// </summary>
    public string BlockName { get; protected set; }
    /// <summary>
    /// The ID value for this block type.
    /// </summary>
    public int ID { get; protected set; }
    /// <summary>
    /// Which of this block type's faces textures should be different from the main texture for this block type.
    /// </summary>
    public UniqueFacesEnum UniqueFaces { get; protected set; }
    /// <summary>
    /// Which of this block type's faces can have their UVs randomly rotated.
    /// </summary>
    public RandomFacesEnum RandomFaces { get; protected set; }
    /// <summary>
    /// The transparency of this block type.
    /// </summary>
    public TransparencyEnum Transparency { get; protected set; }
    /// <summary>
    /// The amount of light this block type emits.
    /// </summary>
    public int LightEmissionValue { get; protected set; }
    /// <summary>
    /// The suffix for this block type's unique top face texture.
    /// </summary>
    public string TopTextureName { get; protected set; }
    /// <summary>
    /// The suffix for this block type's unique bottom face texture.
    /// </summary>
    public string BottomTextureName { get; protected set; }
    /// <summary>
    /// The suffix for this block type's unique front face texture.
    /// </summary>
    public string FrontTextureName { get; protected set; }
    /// <summary>
    /// The suffix for this block type's unique back face texture.
    /// </summary>
    public string BackTextureName { get; protected set; }
    /// <summary>
    /// The suffix for this block type's unique right face texture.
    /// </summary>
    public string RightTextureName { get; protected set; }
    /// <summary>
    /// The suffix for this block type's unique left face texture.
    /// </summary>
    public string LeftTextureName { get; protected set; }


    /// <summary>
    /// How transparent this block type is.
    /// </summary>
    public enum TransparencyEnum
    {
        Transparent,
        SemiTransparent,
        Opaque,
    }

    /// <summary>
    /// Which faces of this block type are unique.
    /// </summary>
    [Flags]
    public enum UniqueFacesEnum
    {
        None = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Front = 1 << 2,
        Back = 1 << 3,
        Right = 1 << 4,
        Left = 1 << 5,
    }

    /// <summary>
    /// Which faces of this block type are randomly rotated.
    /// </summary>
    [Flags]
    public enum RandomFacesEnum
    {
        None = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Front = 1 << 2,
        Back = 1 << 3,
        Right = 1 << 4,
        Left = 1 << 5,
    }
    #endregion Block Data


    /// <summary>
    /// Specific Constructor: Makes a new block type with the given parameters.
    /// </summary>
    /// <param name="blockName">The name of this block type.</param>
    /// <param name="id">The ID of this block type.</param>
    /// <param name="transparency">The transparency of this block type.</param>
    /// <param name="lightEmissionValue">How much light this block type emits.</param>
    /// <param name="uniqueFaces">Which faces of this block type have unique textures.</param>
    /// <param name="randomFaces">Which faces of this block type can be randomly rotated.</param>
    /// <param name="topTextureName">Optional: The name of the unique top texture.</param>
    /// <param name="bottomTextureName">Optional: The name of the unique bottom texture.</param>
    /// <param name="frontTextureName">Optional: The name of the unique front texture.</param>
    /// <param name="backTextureName">Optional: The name of the unique back texture.</param>
    /// <param name="rightTextureName">Optional: The name of the unique right texture.</param>
    /// <param name="leftTextureName">Optional: The name of the unique left texture.</param>
    public BlockType(string blockName, int id, TransparencyEnum transparency, int lightEmissionValue, UniqueFacesEnum uniqueFaces, RandomFacesEnum randomFaces, string topTextureName = "", string bottomTextureName = "", string frontTextureName = "", string backTextureName = "", string rightTextureName = "", string leftTextureName = "")
    {
        this.BlockName = blockName;
        this.ID = id;
        this.Transparency = transparency;
        this.LightEmissionValue = lightEmissionValue;
        this.UniqueFaces = uniqueFaces;
        this.RandomFaces = randomFaces;
        this.TopTextureName = topTextureName;
        this.BottomTextureName = bottomTextureName;
        this.FrontTextureName = frontTextureName;
        this.BackTextureName = backTextureName;
        this.RightTextureName = rightTextureName;
        this.LeftTextureName = leftTextureName;
        BlockTypes.Add(this.ID, this);
    }
}