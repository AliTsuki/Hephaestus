using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// 
/// </summary>
public class OreVein
{
    public Vector3Int Center;
    public List<Vector3Int> Points;

    public OreVein(Vector3Int center)
    {
        this.Center = center;
    }
}
