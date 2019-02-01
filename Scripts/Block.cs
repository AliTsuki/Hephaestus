using UnityEngine;

// Class for Block functions
public class Block : ITickable
{
    // Start of Block List
    public static Block Air = new Block("Air", true, false);
    public static Block Bedrock = new Block("Bedrock", false, "Assets/Resources/Textures/Blocks/Bedrock.png", false);
    public static Block Clay = new Block("Clay", false, "Assets/Resources/Textures/Blocks/Clay.png", false);
    public static Block Cobblestone = new Block("Cobblestone", false, "Assets/Resources/Textures/Blocks/Cobblestone.png", false);
    public static Block Copper_Ore = new Block("Copper Ore", false, "Assets/Resources/Textures/Blocks/Copper_Ore.png", false);
    public static Block Diamond_Ore = new Block("Diamond Ore", false, "Assets/Resources/Textures/Blocks/Diamond_Ore.png", false);
    public static Block Dirt = new Block("Dirt", false, "Assets/Resources/Textures/Blocks/Dirt.png", false);
    public static Block Gold_Ore = new Block("Gold Ore", false, "Assets/Resources/Textures/Blocks/Gold_Ore.png", false);
    public static Block Grass = new Block("Grass", false, "Assets/Resources/Textures/Blocks/Grass_Top.png", "Assets/Resources/Textures/Blocks/Grass_Side.png", "Assets/Resources/Textures/Blocks/Dirt.png", false);
    public static Block Gravel = new Block("Gravel", false, "Assets/Resources/Textures/Blocks/Gravel.png", false);
    public static Block Iron_Ore = new Block("Iron Ore", false, "Assets/Resources/Textures/Blocks/Iron_Ore.png", false);
    public static Block Leaves = new Block("Leaves", false, "Assets/Resources/Textures/Blocks/Leaves.png", false);
    public static Block Logs = new Block("Logs", false, "Assets/Resources/Textures/Blocks/Logs_Top.png", "Assets/Resources/Textures/Blocks/Logs_Side.png", false);
    public static Block Obsidian = new Block("Obsidian", false, "Assets/Resources/Textures/Blocks/Obsidian.png", false);
    public static Block Purple_Ore = new Block("Purple_Ore", false, "Assets/Resources/Textures/Blocks/Purple_Ore.png", false);
    public static Block Red_Ore = new Block("Red_Ore", false, "Assets/Resources/Textures/Blocks/Red_Ore.png", false);
    public static Block Sand = new Block("Sand", false, "Assets/Resources/Textures/Blocks/Sand.png", false);
    public static Block Snow = new Block("Snow", false, "Assets/Resources/Textures/Blocks/Snow.png", false);
    public static Block Stone = new Block("Stone", false, "Assets/Resources/Textures/Blocks/Stone.png", false);
    public static Block Uranium_Ore = new Block("Uranium Ore", false, "Assets/Resources/Textures/Blocks/Uranium_Ore.png", false);
    public static Block Water = new Block("Water", false, "Assets/Resources/Textures/Blocks/Water.png", true);
    public static Block Wood_Planks = new Block("Wood Planks", false, "Assets/Resources/Textures/Blocks/WoodPlanks.png", false);
    // End of Block List

    //Block variables/objects
    private static int currentID = 0;
    private readonly bool isTransparent;
    private readonly bool isSemiTransparent;
    private readonly string bottomImageName;
    private readonly string topImageName;
    private readonly string frontImageName;
    private readonly string backImageName;
    private readonly string leftImageName;
    private readonly string rightImageName;
    private readonly string blockName;
    private int id;
    private readonly Vector2[] _UVMap;
    private readonly Vector2[] _UVMap2;
    private readonly Vector2[] _UVMap3;

    // Block constructor for Blocks with unique top, sides, bottom textures
    public Block(string BlockName, bool IsTransparent, string TopImageName, string SideImageName, string BottomImageName, bool IsSemiTransparent)
    {
        this.blockName = BlockName;
        this.isTransparent = IsTransparent;
        this.topImageName = TopImageName;
        this.leftImageName = SideImageName;
        this.bottomImageName = BottomImageName;
        this._UVMap = UVMap.GetUVMap(this.topImageName)._UVMAP;
        this._UVMap2 = UVMap.GetUVMap(this.leftImageName)._UVMAP;
        this._UVMap3 = UVMap.GetUVMap(this.bottomImageName)._UVMAP;
        this.isSemiTransparent = IsSemiTransparent;
        this.REGISTER();
    }

    // Block constructor for Blocks with unique top, sides, bottom textures
    public Block(string BlockName, bool IsTransparent, string TopImageName, string SideImageName, bool IsSemiTransparent)
    {
        this.blockName = BlockName;
        this.isTransparent = IsTransparent;
        this.topImageName = TopImageName;
        this.leftImageName = SideImageName;
        this._UVMap = UVMap.GetUVMap(this.topImageName)._UVMAP;
        this._UVMap2 = UVMap.GetUVMap(this.leftImageName)._UVMAP;
        this.isSemiTransparent = IsSemiTransparent;
        this.REGISTER();
    }

    // Block constructor for Blocks with same texture on all sides
    public Block(string BlockName, bool IsTransparent, string MainImageName, bool IsSemiTransparent)
    {
        this.blockName = BlockName;
        this.isTransparent = IsTransparent;
        this.topImageName = MainImageName;
        this._UVMap = UVMap.GetUVMap(this.topImageName)._UVMAP;
        this.isSemiTransparent = IsSemiTransparent;
        this.REGISTER();
    }

    // Block constructor for: Transparent Blocks (Air block only)
    public Block(string BlockName, bool IsTransparent, bool IsSemiTransparent)
    {
        this.blockName = BlockName;
        this.isTransparent = IsTransparent;
        this.isSemiTransparent = IsSemiTransparent;
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
        this.id = currentID;
        currentID++;
        BlockRegistry.RegisterBlock(this);
    }

    // Get if block is transparent
    public bool IsTransparent()
    {
        return this.isTransparent;
    }

    // Get if block is semi-transparent
    public bool IsSemiTransparent()
    {
        return this.isSemiTransparent;
    }

    // Get Block name
    public string GetName()
    {
        return this.blockName;
    }

    // Get Block ID
    public int GetID()
    {
        return this.id;
    }

    // Draw Block using MathHelper DrawCube
    public virtual MeshData Draw(Chunk chunk, Block[,,] _Blocks, int x, int y, int z)
    {
        // If block is air, don't bother drawing
        if(this.Equals(Air))
        {
            return new MeshData();
        }
        // If block is NOT air, Draw Cube
        else if(this.Equals(Grass))
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
        else if(this.Equals(Logs))
        {
            try
            {
                return MathHelper.DrawCube(chunk, _Blocks, this, x, y, z, this._UVMap, this._UVMap2);
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
