using System.Collections.Generic;

using UnityEngine;

// Class for creating UVMap
public class UVMap
{
    // UVMap fields
    public static List<UVMap> Maps = new List<UVMap>();
    public string Name;
    public Vector2[] UVMaps;

    // UVMap constructor
    public UVMap(string name, Vector2[] uvMap)
    {
        this.Name = name;
        this.UVMaps = uvMap;
    }

    // Register UVMaps
    public void Register() => Maps.Add(this);

    // Get UVMap
    public static UVMap GetUVMap(string name)
    {
        foreach(UVMap m in Maps)
        {
            if(m.Name.Equals(name))
            {
                return m;
            }
        }
        Debug.Log($@"{GameManager.time}: Can't find associated image: {name}");
        List<string> names = new List<string>
        {
            "Broken Images"
        };
        foreach(UVMap m in Maps)
        {
            names.Add(m.Name + "!=" + name);
        }
        System.IO.File.WriteAllLines("BrokenImageNames.txt", names.ToArray());
        GameManager.ExitGame();
        return Maps[0];
    }
}
