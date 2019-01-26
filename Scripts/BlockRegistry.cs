using System.Collections.Generic;

using UnityEngine;

// Class for creating BlockRegistry
public class BlockRegistry
{
    // BlockRegistry variables/objects
    private static readonly bool DebugMode = true;
    private static List<Block> _REGISTEREDBLOCKS = new List<Block>();

    // Register Block
    public static void RegisterBlock(Block b)
    {
        Debug.Log($@"Registering Block: {b.GetName()}, with ID: {b.GetID()}");
        _REGISTEREDBLOCKS.Add(b);
    }

    // Register All Blocks to BlockRegistry.txt FILE
    public static void RegisterBlocks()
    {
        if(DebugMode)
        {
            int CurrentID = 0;
            List<string> _names = new List<string>();
            Debug.Log("Trying to RegisterBlocks with NULL BlockRegistry!!");
            foreach(Block b in _REGISTEREDBLOCKS)
            {
                _names.Add(string.Format("CurrentID: {0}, BlockName: {1}, BlockID: {2}", CurrentID, b.GetName(), b.GetID()));
                CurrentID++;
            }
            System.IO.File.WriteAllLines("BlockRegistry.txt", _names.ToArray());
        }
    }

    // Get Block object from given ID as Registered
    internal static Block GetBlockFromID(int v)
    {
        try
        {
            if(_REGISTEREDBLOCKS == null)
            {
                Debug.Log("Trying to GetBlockFromID with NULL BlockRegistry!!");
            }
            return _REGISTEREDBLOCKS[v];
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }
}
