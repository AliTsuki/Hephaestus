using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

using UnityEngine;


/// <summary>
/// Class used to save and load chunk data to drive.
/// </summary>
public static class SaveSystem
{
    /// <summary>
    /// The path for world saves.
    /// </summary>
    private const string saveDir = @"Saves";
    /// <summary>
    /// The path for this specific world's save data.
    /// </summary>
    private static string worldSaveDir = "";
    /// <summary>
    /// Dictionary of all chunk save files for this world.
    /// </summary>
    private static readonly Dictionary<Vector3Int, string> chunkSaveFiles = new Dictionary<Vector3Int, string>();


    /// <summary>
    /// Initializes the save system by checking if there is a save directory for all saves, and for this world in particular, then
    /// loops through all chunk save files and keeps a reference to each of them to be loaded on demand later.
    /// </summary>
    public static void InitializeSaveSystem()
    {
        GameManager.Instance.WorldSaveData = new World.WorldSaveData(GameManager.Instance.WorldSaveName, GameManager.Instance.Seed);
        if(Directory.Exists(saveDir) == false)
        {
            Directory.CreateDirectory(saveDir);
        }
        worldSaveDir = saveDir + @"\" + GameManager.Instance.WorldSaveData.WorldSaveName + @"\Chunks\";
        if(Directory.Exists(worldSaveDir) == false)
        {
            Directory.CreateDirectory(worldSaveDir);
        }
        List<string> chunkSaveFilePaths = new List<string>(Directory.GetFiles(worldSaveDir, "*.CHUNK"));
        foreach(string chunkSaveFilePath in chunkSaveFilePaths)
        {
            if(TryConvertChunkSaveFilePathToChunkPos(chunkSaveFilePath, out Vector3Int chunkPos))
            {
                chunkSaveFiles.Add(chunkPos, chunkSaveFilePath);
            }
        }
    }

    /// <summary>
    /// Tries to convert a path name for a chunk save file to a chunk position.
    /// </summary>
    /// <param name="chunkSaveFilePath">The path to parse.</param>
    /// <param name="chunkPos">The output of a chunk position.</param>
    /// <returns>Returns true if successfully parsed chunk pos from path.</returns>
    private static bool TryConvertChunkSaveFilePathToChunkPos(string chunkSaveFilePath, out Vector3Int chunkPos)
    {
        Regex regex = new Regex(@"([-0-9]+)+(?:_)+([-0-9]+)+(?:_)+([-0-9]+)+");
        Match matches = regex.Match(chunkSaveFilePath);
        CaptureCollection cc = matches.Captures;
        if(cc.Count == 3)
        {
            chunkPos = new Vector3Int(int.Parse(cc[0].Value), int.Parse(cc[1].Value), int.Parse(cc[2].Value));
            return true;
        }
        chunkPos = new Vector3Int();
        return false;
    }

    /// <summary>
    /// Converts a chunk position to a chunk save file path to use loading or saving to drive.
    /// </summary>
    /// <param name="chunkPos">The chunk position to convert to file path.</param>
    /// <returns>Returns the file path associated with the chunk at the given position.</returns>
    private static string ConvertChunkPosToSaveFilePath(Vector3Int chunkPos)
    {
        return $@"{worldSaveDir}{chunkPos.x}_{chunkPos.y}_{chunkPos.z}.CHUNK";
    }

    /// <summary>
    /// Saves the given chunk to the drive.
    /// </summary>
    /// <param name="chunk">The chunk to save.</param>
    public static void SaveChunkToDrive(Chunk chunk)
    {
        string path = ConvertChunkPosToSaveFilePath(chunk.ChunkPos);
        SerializeToFile(path, chunk);
        chunkSaveFiles.Add(chunk.ChunkPos, path);
    }

    /// <summary>
    /// Tries to load the chunk from the drive.
    /// </summary>
    /// <param name="chunkPos">The chunk position to look for a saved chunk.</param>
    /// <param name="chunk">The output of the chunk read from file on drive.</param>
    /// <returns>Returns true if there was a file for that chunk and reading it was successful.</returns>
    public static bool TryLoadChunkFromDrive(Vector3Int chunkPos, out Chunk chunk)
    {
        if(DeserializeFromFile(ConvertChunkPosToSaveFilePath(chunkPos), out chunk) == true)
        {
            return true;
        }
        chunk = null;
        return false;
    }

    /// <summary>
    /// Serializes an object to a binary file at the given path.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    /// <param name="path">The path to save the file to drive at.</param>
    /// <param name="data">The object to serialize and save to drive as binary file.</param>
    public static void SerializeToFile<T>(string path, T data) where T : class
    {
        try
        {
            using(Stream stream = File.OpenWrite(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(e);
        }
    }

    /// <summary>
    /// Deserializes the file at the given path and returns the object.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    /// <param name="path">The path to read the file on drive from.</param>
    /// <returns>Returns the object deserialized from the binary file on drive.</returns>
    public static bool DeserializeFromFile<T>(string path, out T data) where T : class
    {
        if(File.Exists(path))
        {
            try
            {
                using(Stream stream = File.OpenRead(path))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data = formatter.Deserialize(stream) as T;
                    return true;
                }
            }
            catch(System.Exception e)
            {
                Logger.Log(e);
            }
        }
        data = null;
        return false;
    }
}
