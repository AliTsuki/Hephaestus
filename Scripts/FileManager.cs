// Class for managing save files
public class FileManager
{
    // FileManager variables
    public static readonly string ChunkSaveDirectory = $@"Data/World/{GameManager.WorldName}/Chunks/";

    // Register Files
    public static void RegisterFiles()
    {
        Serializer.Check_Gen_Folder(ChunkSaveDirectory);
    }

    // Get Chunk Filename string
    public static string GetChunkString(int x, int y, int z)
    {
        return $@"{ChunkSaveDirectory}C_{x}_{y}_{z}.CHUNK";
    }
}
