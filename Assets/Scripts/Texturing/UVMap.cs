using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Struct containing the UVs for a block texture on the texture atlas.
/// </summary>
public struct UVMap
{
    /// <summary>
    /// Dictionary of all UVs for all block textures on the texture atlas.
    /// </summary>
    public static Dictionary<string, UVMap> UVMaps = new Dictionary<string, UVMap>();
    /// <summary>
    /// The name of the block these UVs represent.
    /// </summary>
    public string Name;
    /// <summary>
    /// The UVs for this block texture on the texture atlas.
    /// </summary>
    public Vector2[] UVs;


    /// <summary>
    /// Specific Constructor: Creates a UVMap with the given block texture name and UV coordinates for the block textures on the texture atlas.
    /// </summary>
    /// <param name="name">The name of this block texture.</param>
    /// <param name="uvs">The UVs for this block texture on the texture atlas.</param>
    public UVMap(string name, Vector2[] uvs)
    {
        this.Name = name;
        this.UVs = uvs;
        UVMaps.Add(this.Name, this);
    }

    /// <summary>
    /// Gets UVs corresponding to the given name.
    /// </summary>
    /// <param name="name">The name to find the UVs for.</param>
    /// <returns>Returns the UVs for that name.</returns>
    public static Vector2[] GetUVs(string name)
    {
        if(UVMaps.ContainsKey(name) == true)
        {
            return UVMaps[name].UVs;
        }
        Logger.Log($@"Can't find UVs for texture: {name}");
        return UVMaps["ERROR"].UVs;
    }
}
