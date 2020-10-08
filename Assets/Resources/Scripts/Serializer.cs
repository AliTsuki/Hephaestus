using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OLD
{
    // Class for reading and writing files
    public class Serializer
    {
        // Checks if folder exists, creates if not
        public static bool CheckGenFolder(string _path)
        {
            if(Directory.Exists(_path))
            {
                return true;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(_path);
                    return true;
                }
                catch(System.Exception e)
                {
                    Logger.Log(e);
                }
            }
            return false;
        }

        // Serializes to file
        public static void SerializeToFile<T>(string _path, T _data) where T : class
        {
            try
            {
                using(Stream stream = File.OpenWrite(_path))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, _data);
                }
            }
            catch(System.Exception e)
            {
                Logger.Log(e);
            }
        }

        // Reads from file
        public static T DeserializeFromFile<T>(string _path) where T : class
        {
            if(File.Exists(_path))
            {
                try
                {
                    using(Stream stream = File.OpenRead(_path))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        return formatter.Deserialize(stream) as T;
                    }
                }
                catch(System.Exception e)
                {
                    Logger.Log($@"{GameManager.Time}: {e.ToString()}: Error in Deserialization of: {_path}");
                }
            }
            else
            {
                throw new System.Exception("File can't be found at: " + _path);
            }
            return null;
        }
    }
}
