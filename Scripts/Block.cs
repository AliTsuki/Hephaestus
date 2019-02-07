using UnityEngine;

// Class for Block functions
public class Block : ITickable
{
    // Start of Block List
    public static Block Air = new Block("Air", true, false);
    public static Block Bedrock = new Block("Bedrock", false, false, "Assets/Resources/Textures/Blocks/Bedrock.png");
    public static Block Clay = new Block("Clay", false, false, "Assets/Resources/Textures/Blocks/Clay.png");
    public static Block Cobblestone = new Block("Cobblestone", false, false, "Assets/Resources/Textures/Blocks/Cobblestone.png");
    public static Block Copper_Ore = new Block("Copper Ore", false, false, "Assets/Resources/Textures/Blocks/Copper_Ore.png");
    public static Block Diamond_Ore = new Block("Diamond Ore", false, false, "Assets/Resources/Textures/Blocks/Diamond_Ore.png");
    public static Block Dirt = new Block("Dirt", false, false, "Assets/Resources/Textures/Blocks/Dirt.png");
    public static Block Gold_Ore = new Block("Gold Ore", false, false, "Assets/Resources/Textures/Blocks/Gold_Ore.png");
    public static Block Grass = new Block("Grass", false, false, "Assets/Resources/Textures/Blocks/Grass_Top.png", "Assets/Resources/Textures/Blocks/Grass_Side.png", "Assets/Resources/Textures/Blocks/Dirt.png");
    public static Block Gravel = new Block("Gravel", false, false, "Assets/Resources/Textures/Blocks/Gravel.png");
    public static Block Iron_Ore = new Block("Iron Ore", false, false, "Assets/Resources/Textures/Blocks/Iron_Ore.png");
    public static Block Leaves = new Block("Leaves", false, false, "Assets/Resources/Textures/Blocks/Leaves.png");
    public static Block Logs = new Block("Logs", false, false, "Assets/Resources/Textures/Blocks/Logs_Top.png", "Assets/Resources/Textures/Blocks/Logs_Side.png");
    public static Block Obsidian = new Block("Obsidian", false, false, "Assets/Resources/Textures/Blocks/Obsidian.png");
    public static Block Purple_Ore = new Block("Purple_Ore", false, false, "Assets/Resources/Textures/Blocks/Purple_Ore.png");
    public static Block Red_Ore = new Block("Red_Ore", false, false, "Assets/Resources/Textures/Blocks/Red_Ore.png");
    public static Block Sand = new Block("Sand", false, false, "Assets/Resources/Textures/Blocks/Sand.png");
    public static Block Snow = new Block("Snow", false, false, "Assets/Resources/Textures/Blocks/Snow.png");
    public static Block Stone = new Block("Stone", false, false, "Assets/Resources/Textures/Blocks/Stone.png");
    public static Block Uranium_Ore = new Block("Uranium Ore", false, false, "Assets/Resources/Textures/Blocks/Uranium_Ore.png");
    public static Block Water = new Block("Water", false, true, "Assets/Resources/Textures/Blocks/Water.png");
    public static Block Wood_Planks = new Block("Wood Planks", false, false, "Assets/Resources/Textures/Blocks/WoodPlanks.png");
    // End of Block List

    //Block variables/objects
    private static int currentID = 0;
    public int ID { get; private set; }
    public string BlockName { get; private set; }
    public bool IsTransparent { get; private set; }
    public bool IsSemiTransparent { get; private set; }
    public int LightValue { get; private set; }
    public int Orientation { get; private set; }
    private readonly string bottomImageName;
    private readonly string topImageName;
    private readonly string frontImageName;
    private readonly string backImageName;
    private readonly string leftImageName;
    private readonly string rightImageName;
    private readonly Vector2[] _UVMap1;
    private readonly Vector2[] _UVMap2;
    private readonly Vector2[] _UVMap3;
    private readonly Vector2[] _UVMap4;
    private readonly Vector2[] _UVMap5;
    private readonly Vector2[] _UVMap6;

    // Block constructor for Blocks with unique top, sides, bottom textures (Grass)
    public Block(string BlockName, bool IsTransparent, bool IsSemiTransparent, string TopImageName, string SideImageName, string BottomImageName)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.IsSemiTransparent = IsSemiTransparent;
        this.topImageName = TopImageName;
        this.leftImageName = SideImageName;
        this.bottomImageName = BottomImageName;
        this._UVMap1 = UVMap.GetUVMap(this.topImageName)._UVMAP;
        this._UVMap2 = UVMap.GetUVMap(this.leftImageName)._UVMAP;
        this._UVMap3 = UVMap.GetUVMap(this.bottomImageName)._UVMAP;
        this.REGISTER();
    }

    // Block constructor for Blocks with unique top/bottom and side textures (Logs)
    public Block(string BlockName, bool IsTransparent, bool IsSemiTransparent, string TopImageName, string SideImageName)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.IsSemiTransparent = IsSemiTransparent;
        this.topImageName = TopImageName;
        this.leftImageName = SideImageName;
        this._UVMap1 = UVMap.GetUVMap(this.topImageName)._UVMAP;
        this._UVMap2 = UVMap.GetUVMap(this.leftImageName)._UVMAP;
        this.REGISTER();
    }

    // Block constructor for Blocks with same texture on all sides
    public Block(string BlockName, bool IsTransparent, bool IsSemiTransparent, string MainImageName)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.IsSemiTransparent = IsSemiTransparent;
        this.topImageName = MainImageName;
        this._UVMap1 = UVMap.GetUVMap(this.topImageName)._UVMAP;
        this.REGISTER();
    }

    // Block constructor for Transparent Blocks (Air block only)
    public Block(string BlockName, bool IsTransparent, bool IsSemiTransparent)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.IsSemiTransparent = IsSemiTransparent;
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

    // Draw Block using MathHelper DrawCube
    public MeshData Draw(int x, int y, int z, Block[,,] blocks, int PosX, int PosY, int PosZ)
    {
        // If block is air, don't bother drawing
        if(this.Equals(Air))
        {
            return new MeshData();
        }
        // If block is Grass draw with special rules (3 UVs)
        else if(this.Equals(Grass))
        {
            try
            {
                return MathHelper.DrawCubeGrass(x, y, z, blocks, this, this._UVMap1, this._UVMap2, this._UVMap3, PosX, PosY, PosZ);
            }
            catch(System.Exception e)
            {
                Debug.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
                Logger.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
            }
            return new MeshData();
        }
        // If block is Logs draw with special rules (2 UVs)
        else if(this.Equals(Logs))
        {
            try
            {
                return MathHelper.DrawCubeLogs(x, y, z, blocks, this, this._UVMap1, this._UVMap2, PosX, PosY, PosZ);
            }
            catch(System.Exception e)
            {
                Debug.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
                Logger.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
            }
            return new MeshData();
        }
        // If block is anything else draw with normal rules (1 UV)
        else
        {
            try
            {
                return MathHelper.DrawCube(x, y, z, blocks, this, this._UVMap1, PosX, PosY, PosZ);
            }
            catch(System.Exception e)
            {
                Debug.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
                Logger.Log($@"Error in Drawing Cube at X:{x}, Y:{y}, Z:{z} ERROR:{e.ToString()}");
            }
            return new MeshData();
        }
    } 
}
