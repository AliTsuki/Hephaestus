using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for Block functions
public class Block : ITickable
{
    // Start of Block List
    public static Block Bedrock = new Block("Bedrock", false, "Assets/Resources/Textures/Blocks/Bedrock.png");
    public static Block Grass = new Block("Grass", false, "Assets/Resources/Textures/Blocks/Grass_Top.png");
    public static Block Stone = new Block("Stone", false, "Assets/Resources/Textures/Blocks/Stone.png");
    public static Block Dirt = new Block("Dirt", false, "Assets/Resources/Textures/Blocks/Dirt.png");
    public static Block Air = new Block("Air", true);
    // End of Block List

    //Block variables/objects
    private static int CurrentID = 0;
    private readonly bool IsTransparent;
    private readonly string ImageName;
    private readonly string BlockName;
    private int ID;
    private readonly Vector2[] _UVMap;

    // Block constructor for BlockName, IsTransparent, name (All but Air blocks)
    public Block(string BlockName, bool IsTransparent, string ImageName)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.ImageName = ImageName;
        this._UVMap = UVMap.GetUVMap(this.ImageName)._UVMAP;
        this.REGISTER();
    }

    // Block constructor for: IsTransparent (Air block only)
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
        try
        {
            return MathHelper.DrawCube(chunk, _Blocks, this, x, y, z, this._UVMap);
        }
        catch(System.Exception e)
        {
            Debug.Log("Error in Drawing Cube at: " + x + y + z + e.ToString());
        }
        return new MeshData();
    } 
}
