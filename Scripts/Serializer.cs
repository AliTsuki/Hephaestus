using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// Class for reading and writing files
public class Serializer
{
    // Checks if folder exists, creates if not
    public static bool Check_Gen_Folder(string path)
    {
        if(Directory.Exists(path))
        {
            return true;
        }
        else
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch(System.Exception e)
            {
                Logger.MainLog.log(e.ToString());
            }
        }
        return false;
    }

    // Serializes to file
    public static void Serialize_ToFile_FullPath<T>(string path, T _DATA) where T : class
    {
        try
        {
            using(Stream s = File.OpenWrite(path))
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(s, _DATA);
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(e.ToString());
        }
    }

    // Reads from file
    public static T Deserialize_From_File<T>(string path) where T : class
    {
        if(File.Exists(path))
        {
            try
            {
                using(Stream s = File.OpenRead(path))
                {
                    BinaryFormatter f = new BinaryFormatter();
                    return f.Deserialize(s) as T;
                }
            }
            catch(System.Exception e)
            {
                Logger.Log(e.ToString() + "Error in Deserialization of: " + path);
            }
        }
        else
        {
            throw new System.Exception("File cannot be found at: " + path);
        }
        return null;
    }
}
