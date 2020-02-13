using UnityEngine;

// Class for Block functions
public class Block : ITickable
{
    // Start of Block List
    public static Block Air =           new Block("Air",            true,  false, BlockFaceStyle.AirStyle,       null, null, null, null, null, null);
    public static Block Bedrock =       new Block("Bedrock",        false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Bedrock.png", null, null, null, null, null);
    public static Block Clay =          new Block("Clay",           false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Clay.png", null, null, null, null, null);
    public static Block Cobblestone =   new Block("Cobblestone",    false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Cobblestone.png", null, null, null, null, null);
    public static Block Copper_Ore =    new Block("Copper Ore",     false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Copper_Ore.png", null, null, null, null, null);
    public static Block Diamond_Ore =   new Block("Diamond Ore",    false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Diamond_Ore.png", null, null, null, null, null);
    public static Block Dirt =          new Block("Dirt",           false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Dirt.png", null, null, null, null, null);
    public static Block Gold_Ore =      new Block("Gold Ore",       false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Gold_Ore.png", null, null, null, null, null);
    public static Block Grass =         new Block("Grass",          false, false, BlockFaceStyle.GrassStyle,     "Assets/Resources/Textures/Blocks/Grass_Top.png", "Assets/Resources/Textures/Blocks/Grass_Side.png", "Assets/Resources/Textures/Blocks/Dirt.png", null, null, null);
    public static Block Gravel =        new Block("Gravel",         false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Gravel.png", null, null, null, null, null);
    public static Block Iron_Ore =      new Block("Iron Ore",       false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Iron_Ore.png", null, null, null, null, null);
    public static Block Leaves =        new Block("Leaves",         false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Leaves.png", null, null, null, null, null);
    public static Block Logs =          new Block("Logs",           false, false, BlockFaceStyle.LogStyle,       "Assets/Resources/Textures/Blocks/Logs_Top.png", "Assets/Resources/Textures/Blocks/Logs_Side.png", null, null, null, null);
    public static Block Obsidian =      new Block("Obsidian",       false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Obsidian.png", null, null, null, null, null);
    public static Block Purple_Ore =    new Block("Purple_Ore",     false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Purple_Ore.png", null, null, null, null, null);
    public static Block Red_Ore =       new Block("Red_Ore",        false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Red_Ore.png", null, null, null, null, null);
    public static Block Sand =          new Block("Sand",           false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Sand.png", null, null, null, null, null);
    public static Block Snow =          new Block("Snow",           false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Snow.png", null, null, null, null, null);
    public static Block Stone =         new Block("Stone",          false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Stone.png", null, null, null, null, null);
    public static Block Uranium_Ore =   new Block("Uranium Ore",    false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Uranium_Ore.png", null, null, null, null, null);
    public static Block Water =         new Block("Water",          false, true,  BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/Water.png", null, null, null, null, null);
    public static Block Wood_Planks =   new Block("Wood Planks",    false, false, BlockFaceStyle.UniversalStyle, "Assets/Resources/Textures/Blocks/WoodPlanks.png", null, null, null, null, null);
    // End of Block List

    //Block fields
    private static int currentID = 0;
    public int ID { get; private set; }
    public string BlockName { get; private set; }
    public bool IsTransparent { get; private set; }
    public bool IsSemiTransparent { get; private set; }
    public int LightValue { get; private set; }
    public int Orientation { get; private set; }
    private readonly BlockFaceStyle style;
    private readonly string textureName0;
    private readonly string textureName1;
    private readonly string textureName2;
    private readonly string textureName3;
    private readonly string textureName4;
    private readonly string textureName5;
    private readonly Vector2[] uvMap0;
    private readonly Vector2[] uvMap1;
    private readonly Vector2[] uvMap2;
    private readonly Vector2[] uvMap3;
    private readonly Vector2[] uvMap4;
    private readonly Vector2[] uvMap5;
    public enum BlockFaceStyle
    {
        AirStyle,
        UniversalStyle,
        LogStyle,
        GrassStyle
    };
    
    // Block constructor for Blocks with unique top, sides, bottom textures (Grass)
    public Block(string _blockName, bool _isTransparent, bool _isSemiTransparent, BlockFaceStyle _style, string _textureName0, string _textureName1, string _textureName2, string _textureName3, string _textureName4, string _textureName5)
    {
        this.BlockName = _blockName;
        this.IsTransparent = _isTransparent;
        this.IsSemiTransparent = _isSemiTransparent;
        this.style = _style;
        if(this.style == BlockFaceStyle.AirStyle)
        {
            this.textureName0 = null;
            this.textureName1 = null;
            this.textureName2 = null;
            this.textureName3 = null;
            this.textureName4 = null;
            this.textureName5 = null;
            this.uvMap0 = null;
            this.uvMap1 = null;
            this.uvMap2 = null;
            this.uvMap3 = null;
            this.uvMap4 = null;
            this.uvMap5 = null;
        }
        else if(this.style == BlockFaceStyle.UniversalStyle)
        {
            this.textureName0 = _textureName0;
            this.textureName1 = null;
            this.textureName2 = null;
            this.textureName3 = null;
            this.textureName4 = null;
            this.textureName5 = null;
            this.uvMap0 = UVMap.GetUVMap(this.textureName0).UVMaps;
            this.uvMap1 = null;
            this.uvMap2 = null;
            this.uvMap3 = null;
            this.uvMap4 = null;
            this.uvMap5 = null;
        }
        else if(this.style == BlockFaceStyle.LogStyle)
        {
            this.textureName0 = _textureName0;
            this.textureName1 = _textureName1;
            this.textureName2 = null;
            this.textureName3 = null;
            this.textureName4 = null;
            this.textureName5 = null;
            this.uvMap0 = UVMap.GetUVMap(this.textureName0).UVMaps;
            this.uvMap1 = UVMap.GetUVMap(this.textureName1).UVMaps;
            this.uvMap2 = null;
            this.uvMap3 = null;
            this.uvMap4 = null;
            this.uvMap5 = null;
        }
        else if(this.style == BlockFaceStyle.GrassStyle)
        {
            this.textureName0 = _textureName0;
            this.textureName1 = _textureName1;
            this.textureName2 = _textureName2;
            this.textureName3 = null;
            this.textureName4 = null;
            this.textureName5 = null;
            this.uvMap0 = UVMap.GetUVMap(this.textureName0).UVMaps;
            this.uvMap1 = UVMap.GetUVMap(this.textureName1).UVMaps;
            this.uvMap2 = UVMap.GetUVMap(this.textureName2).UVMaps;
            this.uvMap3 = null;
            this.uvMap4 = null;
            this.uvMap5 = null;
        }
        this.REGISTER();
    }

    // Instantiate Blocks
    // TODO: HACKY, maybe find a better solution, calling this forces blocks to register early so 
    // loading chunks from file before generating new chunks doesn't error out at null BlockRegistry
    public static void Instantiate()
    {

    }

    // Block Start
    public void Start()
    {

    }

    // Block Tick
    public void Tick()
    {

    }

    // Block Update
    public void Update()
    {

    }

    // Block On Unity Update
    public void OnUnityUpdate()
    {

    }

    // Register block to Block Registry
    private void REGISTER()
    {
        this.ID = currentID;
        currentID++;
        BlockRegistry.RegisterBlock(this);
    }

    // Draw Block using MeshBuilder DrawCube
    public MeshData Draw(int _x, int _y, int _z, Block[,,] _blocks, Int3 _chunkPos)
    {
        // If block is air, don't bother drawing
        if(this.Equals(Air))
        {
            return new MeshData();
        }
        // If block is anything else draw it
        else
        {
            return MeshBuilder.DrawCube(_x, _y, _z, _blocks, this, this.style, this.uvMap0, this.uvMap1, this.uvMap2, this.uvMap3, this.uvMap4, this.uvMap5, _chunkPos);
        }
    } 
}
