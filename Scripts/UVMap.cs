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
    public void Register()
    {
        Maps.Add(this);
    }

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
        throw new System.Exception($@"Can't find UVMap for {name}");
    }
}
