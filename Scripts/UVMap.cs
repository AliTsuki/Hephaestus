using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for creating UVMap
public class UVMap
{
    // UVMap objects
    public static List<UVMap> _Maps = new List<UVMap>();
    public string name;
    public Vector2[] _UVMAP;

    // UVMap constructor
    public UVMap(string name, Vector2[] _UVMAP)
    {
        this.name = name;
        this._UVMAP = _UVMAP;
    }

    // Register UVMaps
    public void Register() => _Maps.Add(this);

    // Get UVMap
    public static UVMap GetUVMap(string name)
    {
        foreach(UVMap m in _Maps)
        {
            if(m.name.Equals(name))
            {
                return m;
            }
        }
        Debug.Log("Cant find associated image called: " + name);

        List<string> _names = new List<string>
        {
            "Broken Images"
        };
        foreach(UVMap m in _Maps)
        {
            _names.Add(m.name + "!=" + name);
        }
        System.IO.File.WriteAllLines("names.txt", _names.ToArray());
        GameManager.ExitGame();
        return _Maps[0];
    }
}
