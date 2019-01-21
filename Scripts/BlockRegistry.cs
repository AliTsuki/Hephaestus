using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for creating BlockRegistry
public class BlockRegistry
{
    // BlockRegistry variables/objects
    private static readonly bool DebugMode = false;
    private static List<Block> _REGISTEREDBLOCKS = new List<Block>();

    // Register Block
    public static void RegisterBlock(Block b)
    {
        _REGISTEREDBLOCKS.Add(b);
    }

    // Register All Blocks to BlockRegistry.txt FILE
    public static void RegisterBlocks()
    {
        if(DebugMode)
        {
            int i = 0;
            List<string> _names = new List<string>();
            foreach(Block b in _REGISTEREDBLOCKS)
            {
                _names.Add(string.Format("CurrentID: {0}, BlockName: {1}, BlockID: {2}", i, b.GetName(), b.GetID()));
                i++;
            }
            System.IO.File.WriteAllLines("BlockRegistry.txt", _names.ToArray());
        }
    }

    // Get Block object from given ID as Registered
    internal static Block GetBlockFromID(int v)
    {
        try
        {
            return _REGISTEREDBLOCKS[v];
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }
}
