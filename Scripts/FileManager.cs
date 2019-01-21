using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for managing save files
public class FileManager
{
    // FileManager variables
    public static readonly string ChunkSaveDirectory = "Data/World/DevWorld/Chunks/";

    // Register Files
    public static void RegisterFiles()
    {
        Serializer.Check_Gen_Folder(ChunkSaveDirectory);
    }

    // Get Chunk Filename string
    public static string GetChunkString(int x, int z)
    {
        return string.Format("{0}C_{1}_{2}.CHK", ChunkSaveDirectory, x, z);
    }
}
