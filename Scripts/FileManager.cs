// Class for managing save files
public class FileManager
{
    // FileManager fields
    public static readonly string ChunkSaveDirectory = $@"Data/World/{GameManager.WorldName}/Chunks/";

    // Register Files
    public static void RegisterFiles()
    {
        Serializer.CheckGenFolder(ChunkSaveDirectory);
    }

    // Get Chunk Filename string
    public static string GetChunkString(Int3 _pos)
    {
        return $@"{ChunkSaveDirectory}C_{_pos.x}_{_pos.y}_{_pos.z}.CHUNK";
    }
}
