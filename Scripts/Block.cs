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
    public static Block Air = new Block(true);
    // End of Block List

    private readonly bool IsTransparent;
    private readonly string name;
    private readonly Vector2[] _UVMap;
    private static int CurrentID = 0;
    private int ID;
    private readonly string BlockName;

    public Block(string BlockName, bool IsTransparent, string name)
    {
        this.BlockName = BlockName;
        this.IsTransparent = IsTransparent;
        this.name = name;

        this._UVMap = UVMap.GetUVMap(name)._UVMAP;

        this.REGISTER();
    }

    public Block(string name, bool IsTransparent)
    {
        this.BlockName = name;
        this.IsTransparent = IsTransparent;

        this.REGISTER();
    }

    public Block(bool IsTransparent)
    {
        this.IsTransparent = IsTransparent;

        this.REGISTER();
    }

    public bool Istransparent() => this.IsTransparent;

    private void REGISTER()
    {
        this.ID = CurrentID;
        CurrentID++;
        BlockRegistry.RegisterBlock(this);
    }

    public string GetName() => this.BlockName;

    public int GetID() => this.ID;

    public void Start()
    {

    }

    public void Tick()
    {

    }

    public void Update()
    {

    }

    public void OnUnityUpdate()
    {

    }

    public virtual MeshData Draw(Chunk chunk, Block[,,] _Blocks, int x, int y, int z)
    {
        if (this.Equals(Air))
        {
            return new MeshData();
        }
        try
        {
            return MathHelper.DrawCube(chunk, _Blocks, this, x, y, z, this._UVMap);
        }
        catch(System.Exception e)
        {
            Debug.Log("Error in Drawing Cube: " + e.StackTrace.ToString());
        }
        return new MeshData();
    } 
}
