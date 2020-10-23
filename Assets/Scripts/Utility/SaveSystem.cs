using ProtoBuf;

using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;


/// <summary>
/// Class used to save and load column data to drive.
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
    /// Initializes the save system by checking if there is a save directory for all saves, and for this world in particular, then
    /// loops through all column save files and keeps a reference to each of them to be loaded on demand later.
    /// </summary>
    public static void InitializeSaveSystem()
    {
        GameManager.Instance.WorldSaveData = new World.WorldSaveData(GameManager.Instance.WorldSaveName, GameManager.Instance.Seed);
        if(Directory.Exists(saveDir) == false)
        {
            Directory.CreateDirectory(saveDir);
        }
        worldSaveDir = saveDir + @"\" + GameManager.Instance.WorldSaveData.WorldSaveName + @"\Columns\";
        if(Directory.Exists(worldSaveDir) == false)
        {
            Directory.CreateDirectory(worldSaveDir);
        }
    }

    /// <summary>
    /// Saves the given column to the drive.
    /// </summary>
    /// <param name="column">The column to save.</param>
    public static void SaveColumnToDrive(Column column)
    {
        string path = ConvertColumnPosToSaveFilePath(column.ColumnPos);
        SerializeToFile(path, new SaveDataObjects.ColumnSaveData(column));
    }

    /// <summary>
    /// Tries to load the column from the drive.
    /// </summary>
    /// <param name="columnPos">The column position to look for a saved column.</param>
    /// <param name="column">The output of the column read from file on drive.</param>
    /// <returns>Returns true if there was a file for that column and reading it was successful.</returns>
    public static bool TryLoadColumnFromDrive(Vector2Int columnPos, out Column column)
    {
        string path = ConvertColumnPosToSaveFilePath(columnPos);
        if(DeserializeFromFile(path, out SaveDataObjects.ColumnSaveData columnData) == true)
        {
            column = new Column(columnData);
            return true;
        }
        column = null;
        return false;
    }

    /// <summary>
    /// Tries to convert a path name for a column save file to a column position.
    /// </summary>
    /// <param name="columnSaveFilePath">The path to parse.</param>
    /// <param name="columnPos">The output of a column position.</param>
    /// <returns>Returns true if successfully parsed column pos from path.</returns>
    private static bool TryConvertColumnSaveFilePathToColumnPos(string columnSaveFilePath, out Vector2Int columnPos)
    {
        Regex regex = new Regex(@"([-0-9]+)+(?:_)+([-0-9]+)+");
        Match matches = regex.Match(columnSaveFilePath);
        CaptureCollection cc = matches.Captures;
        if(cc.Count == 2)
        {
            columnPos = new Vector2Int(int.Parse(cc[0].Value), int.Parse(cc[1].Value));
            return true;
        }
        columnPos = new Vector2Int();
        return false;
    }

    /// <summary>
    /// Converts a column position to a column save file path to use loading or saving to drive.
    /// </summary>
    /// <param name="columnPos">The column position to convert to file path.</param>
    /// <returns>Returns the file path associated with the column at the given position.</returns>
    private static string ConvertColumnPosToSaveFilePath(Vector2Int columnPos)
    {
        return $@"{worldSaveDir}{columnPos.x}_{columnPos.y}.COLUMN";
    }

    /// <summary>
    /// Serializes an object to a binary file at the given path.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    /// <param name="path">The path to save the file to drive at.</param>
    /// <param name="data">The object to serialize and save to drive as binary file.</param>
    private static void SerializeToFile<T>(string path, T data) where T : struct
    {
        try
        {
            using(FileStream file = File.Create(path))
            {
                Serializer.Serialize(file, data);
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
    private static bool DeserializeFromFile<T>(string path, out T data) where T : struct
    {
        if(File.Exists(path))
        {
            try
            {
                using(FileStream file = File.OpenRead(path))
                {
                    data = Serializer.Deserialize<T>(file);
                    return true;
                }
            }
            catch(System.Exception e)
            {
                Logger.Log(e);
            }
        }
        data = default;
        return false;
    }
}
