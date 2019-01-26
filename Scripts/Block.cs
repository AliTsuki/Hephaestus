using UnityEngine;

// Class for Block functions
public class Block : ITickable
{
    // Start of Block List
    public static Block Air = new Block("Air", true);
    public static Block Bedrock = new Block("Bedrock", false, "Assets/Resources/Textures/Blocks/Bedrock.png");
    public static Block Clay = new Block("Clay", false, "Assets/Resources/Textures/Blocks/Clay.png");
    public static Block Cobblestone = new Block("Cobblestone", false, "Assets/Resources/Textures/Blocks/Cobblestone.png");
    public static Block Copper_Ore = new Block("Copper Ore", false, "Assets/Resources/Textures/Blocks/Copper_Ore.png");
    public static Block Diamond_Ore = new Block("Diamond Ore", false, "Assets/Resources/Textures/Blocks/Diamond_Ore.png");
    public static Block Dirt = new Block("Dirt", false, "Assets/Resources/Textures/Blocks/Dirt.png");
    public static Block Gold_Ore = new Block("Gold Ore", false, "Assets/Resources/Textures/Blocks/Gold_Ore.png");
    public static Block Grass = new Block("Grass", false, "Assets/Resources/Textures/Blocks/Grass_Top.png", "Assets/Resources/Textures/Blocks/Grass_Side.png", "Assets/Resources/Textures/Blocks/Dirt.png");
    public static Block Gravel = new Block("Gravel", false, "Assets/Resources/Textures/Blocks/Gravel.png");
    public static Block Iron_Ore = new Block("Iron Ore", false, "Assets/Resources/Textures/Blocks/Iron_Ore.png");
    public static Block Leaves = new Block("Leaves", false, "Assets/Resources/Textures/Blocks/Leaves.png");
    public static Block Logs = new Block("Logs", false, "Assets/Resources/Textures/Blocks/Logs_Top.png", "Assets/Resources/Textures/Blocks/Logs_Side.png", "Assets/Resources/Textures/Blocks/Logs_Top.png");
    public static Block Obsidian = new Block("Obsidian", false, "Assets/Resources/Textures/Blocks/Obsidian.png");
    public static Block Purple_Ore = new Block("Purple_Ore", false, "Assets/Resources/Textures/Blocks/Purple_Ore.png");
    public static Block Red_Ore = new Block("Red_Ore", false, "Assets/Resources/Textures/Blocks/Red_Ore.png");
    public static Block Sand = new Block("Sand", false, "Assets/Resources/Textures/Blocks/Sand.png");
    public static Block Stone = new Block("Stone", false, "Assets/Resources/Textures/Blocks/Stone.png");
    public static Block Uranium_Ore = new Block("Uranium Ore", false, "Assets/Resources/Textures/Blocks/Uranium_Ore.png");
    public static Block Wood_Planks = new Block("Wood Planks", false, "Assets/Resources/Textures/Blocks/WoodPlanks.png");
    // End of Block List

    //Block variables/objects
    private static int CurrentID = 0;
    private readonly bool IsTransparent;
    private readonly string BottomImageName;
    private readonly string TopImageName;
    private readonly string FrontImageName;
    private readonly string BackImageName;
    private readonly string LeftImageName;
    private readonly string RightImageName;
    private readonly string BlockName;
    private int ID;
    private readonly Vector2[] _UVMap;
    private readonly Vector2[] _UVMap2;
    private readonly Vector2[] _UVMap3;

    // Block constructor for Blocks with unique top, sides, bottom textures
    public Block(string BlockName, bool IsTransparent, string TopImageName, string SideImageName, string BottomImageName)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.TopImageName = TopImageName;
        this.LeftImageName = SideImageName;
        this.BottomImageName = BottomImageName;
        this._UVMap = UVMap.GetUVMap(this.TopImageName)._UVMAP;
        this._UVMap2 = UVMap.GetUVMap(this.LeftImageName)._UVMAP;
        this._UVMap3 = UVMap.GetUVMap(this.BottomImageName)._UVMAP;
        this.REGISTER();
    }

    // Block constructor for Blocks with same texture on all sides
    public Block(string BlockName, bool IsTransparent, string MainImageName)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.TopImageName = MainImageName;
        this._UVMap = UVMap.GetUVMap(this.TopImageName)._UVMAP;
        this.REGISTER();
    }

    // Block constructor for: Transparent Blocks (Air block only)
    public Block(string BlockName, bool IsTransparent)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
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
        this.ID = CurrentID;
        CurrentID++;
        BlockRegistry.RegisterBlock(this);
    }

    // Get if block is transparent
    public bool Istransparent() => this.IsTransparent;

    // Get Block name
    public string GetName() => this.BlockName;

    // Get Block ID
    public int GetID() => this.ID;

    // Draw Block using MathHelper DrawCube
    public virtual MeshData Draw(Chunk chunk, Block[,,] _Blocks, int x, int y, int z)
    {
        // If block is air, don't bother drawing
        if(this.Equals(Air))
        {
            return new MeshData();
        }
        // If block is NOT air, Draw Cube
        else if(this.Equals(Grass) || this.Equals(Logs))
        {
            try
            {
                return MathHelper.DrawCube(chunk, _Blocks, this, x, y, z, this._UVMap, this._UVMap2, this._UVMap3);
            }
            catch(System.Exception e)
            {
                Debug.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
            }
            return new MeshData();
        }
        else
        {
            try
            {
                return MathHelper.DrawCube(chunk, _Blocks, this, x, y, z, this._UVMap);
            }
            catch(System.Exception e)
            {
                Debug.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
            }
            return new MeshData();
        }
    } 
}
