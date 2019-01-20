using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for managing save files
public class FileManager
{
    public static readonly string ChunkSaveDirectory = "Data/World/DevWorld/Chunks/";

    public static void RegisterFiles()
    {
        Serializer.Check_Gen_Folder(ChunkSaveDirectory);
    }

    public static string GetChunkString(int x, int z)
    {
        return string.Format("{0}C{1}_{2}.CHK", ChunkSaveDirectory, x, z);
    }
}
