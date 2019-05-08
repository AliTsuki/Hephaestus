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
    public UVMap(string _name, Vector2[] _uvMap)
    {
        this.Name = _name;
        this.UVMaps = _uvMap;
    }

    // Register UVMaps
    public void Register()
    {
        Maps.Add(this);
    }

    // Get UVMap
    public static UVMap GetUVMap(string _name)
    {
        foreach(UVMap map in Maps)
        {
            if(map.Name.Equals(_name))
            {
                return map;
            }
        }
        Debug.Log($@"{GameManager.Time}: Can't find associated image: {_name}");
        throw new System.Exception($@"Can't find UVMap for {_name}");
    }
}
