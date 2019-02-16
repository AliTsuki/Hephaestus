using System.Collections.Generic;

using UnityEngine;

// Class for creating BlockRegistry
public class BlockRegistry
{
    // BlockRegistry variables/objects
    private static List<Block> registeredBlocks = new List<Block>();

    // Register Block
    public static void RegisterBlock(Block b)
    {
        registeredBlocks.Add(b);
    }

    // Register All Blocks to BlockRegistry.txt FILE
    public static void RegisterBlocks()
    {
        if(GameManager.Instance.IsDebug)
        {
            int CurrentID = 0;
            List<string> names = new List<string>();
            foreach(Block b in registeredBlocks)
            {
                names.Add($@"CurrentID: {CurrentID}, BlockName: {b.BlockName}, BlockID: {b.ID}");
            }
            System.IO.File.WriteAllLines("BlockRegistry.txt", names.ToArray());
        }
    }

    // Get Block object from given ID as Registered
    internal static Block GetBlockFromID(int v)
    {
        try
        {
            if(registeredBlocks == null)
            {
                Debug.Log($@"{GameManager.time}: Trying to GetBlockFromID with NULL BlockRegistry!");
                Logger.Log($@"{GameManager.time}: Trying to GetBlockFromID with NULL BlockRegistry!");
            }
            return registeredBlocks[v];
        }
        catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }
}
