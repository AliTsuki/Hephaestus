using UnityEngine;

using static SaveDataObjects;


/// <summary>
/// Contains extension methods for system/unity structs.
/// </summary>
public static class Extensions
{
    /////////////////////////////////////////////////////////////////////////////////
    /// Vector3Int
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a chunk position to the world position for the center of that chunk.
    /// </summary>
    /// <param name="chunkPos">The chunk position in chunk coordinate system.</param>
    /// <returns>Returns the position in world coordinate system representing the center of the chunk.</returns>
    public static Vector3Int ChunkPosToWorldPos(this Vector3Int chunkPos)
    {
        Vector3Int worldPos = new Vector3Int
        {
            x = chunkPos.x * GameManager.Instance.ChunkSize,
            y = chunkPos.y * GameManager.Instance.ChunkSize,
            z = chunkPos.z * GameManager.Instance.ChunkSize
        };
        return worldPos;
    }

    /// <summary>
    /// Converts an internal position to a world position using the chunk position the internal position is relative to.
    /// </summary>
    /// <param name="internalPos">The internal position of a chunk data value.</param>
    /// <param name="chunkPos">The position of the parent chunk in chunk coordinate system.</param>
    /// <returns>Returns the position in world coordinate system that corresponds to the given chunk internal coordinate system position.</returns>
    public static Vector3Int InternalPosToWorldPos(this Vector3Int internalPos, Vector3Int chunkPos)
    {
        Vector3Int worldPos = new Vector3Int
        {
            x = internalPos.x + (chunkPos.x * GameManager.Instance.ChunkSize),
            y = internalPos.y + (chunkPos.y * GameManager.Instance.ChunkSize),
            z = internalPos.z + (chunkPos.z * GameManager.Instance.ChunkSize)
        };
        return worldPos;
    }

    /// <summary>
    /// Converts a world position to the chunk position of the chunk that contains that world position within its bounds.
    /// Note: if world position belongs to multiple chunks (face/edge/corner) it only returns the first matching chunk.
    /// </summary>
    /// <param name="worldPos">The world position in world coordinate system.</param>
    /// <returns>Returns the position in chunk coordinate system for the chunk that contains the given world position in its bounds.</returns>
    public static Vector3Int WorldPosToChunkPos(this Vector3Int worldPos)
    {
        Vector3Int chunkPos = new Vector3Int(worldPos.x / GameManager.Instance.ChunkSize, worldPos.y / GameManager.Instance.ChunkSize, worldPos.z / GameManager.Instance.ChunkSize);
        if(worldPos.x < 0 && worldPos.x % GameManager.Instance.ChunkSize < 0)
        {
            chunkPos.x -= 1;
        }
        if(worldPos.y < 0 && worldPos.y % GameManager.Instance.ChunkSize < 0)
        {
            chunkPos.y -= 1;
        }
        if(worldPos.z < 0 && worldPos.z % GameManager.Instance.ChunkSize < 0)
        {
            chunkPos.z -= 1;
        }
        return chunkPos;
    }

    /// <summary>
    /// Converts a world position to the chunk internal position within the chunk that contains that world position within its bounds.
    /// Note: if world position belongs to multiple chunks (face/edge/corner) it only returns the internal position for the first matching chunk.
    /// </summary>
    /// <param name="worldPos">The world position in world coordinate system.</param>
    /// <returns>Returns the chunk internal position corresponding to the given world position.</returns>
    public static Vector3Int WorldPosToInternalPos(this Vector3Int worldPos)
    {
        Vector3Int internalPos = new Vector3Int(worldPos.x / GameManager.Instance.ChunkSize, worldPos.y / GameManager.Instance.ChunkSize, worldPos.z / GameManager.Instance.ChunkSize);
        if(worldPos.x < 0 && worldPos.x % GameManager.Instance.ChunkSize < 0)
        {
            internalPos.x -= 1;
        }
        internalPos.x *= GameManager.Instance.ChunkSize;
        if(worldPos.y < 0 && worldPos.y % GameManager.Instance.ChunkSize < 0)
        {
            internalPos.y -= 1;
        }
        internalPos.y *= GameManager.Instance.ChunkSize;
        if(worldPos.z < 0 && worldPos.z % GameManager.Instance.ChunkSize < 0)
        {
            internalPos.z -= 1;
        }
        internalPos.z *= GameManager.Instance.ChunkSize;
        internalPos = new Vector3Int
        {
            x = worldPos.x - internalPos.x,
            y = worldPos.y - internalPos.y,
            z = worldPos.z - internalPos.z
        };
        return internalPos;
    }

    /// <summary>
    /// Gets the manhattan distance from this Vector3Int to the given Vector3Int.
    /// </summary>
    /// <param name="from">The position to measure from.</param>
    /// <param name="to">The position to measure to.</param>
    /// <returns>Returns the manhattan distance (the absolute value of each component in "from" subtracted from "to" added together).</returns>
    public static int ManhattanDistance(this Vector3Int from, Vector3Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) + Mathf.Abs(from.z - to.z);
    }

    /// <summary>
    /// Converts a Vector3Int to a normalized unit Vector3.
    /// </summary>
    /// <param name="v3">The Vector3Int to normalize.</param>
    /// <returns>Returns the normalized unit Vector2.</returns>
    public static Vector3 Normalize(this Vector3Int v3)
    {
        float sqr = Mathf.Sqrt((v3.x * v3.x) + (v3.y * v3.y) + (v3.z * v3.z));
        return new Vector3(v3.x / sqr, v3.y / sqr, v3.z / sqr);
    }

    /// <summary>
    /// Converts a Vector3Int to a Vector2Int by removing the Y component.
    /// </summary>
    /// <param name="v3">The Vector3Int to convert.</param>
    /// <returns>Returns the given Vector3Int as a Vector2 by removing the Y component.</returns>
    public static Vector2Int RemoveY(this Vector3Int v3)
    {
        return new Vector2Int(v3.x, v3.z);
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// Vector3
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Vector3 to a Vector3Int by rounding the x, y, and z using Mathf.RoundToInt on each value.
    /// </summary>
    /// <param name="v3">The Vector3 to convert to a Vector3Int.</param>
    /// <returns>Returns the rounded to integer version of the given Vector3.</returns>
    public static Vector3Int RoundToInt(this Vector3 v3)
    {
        return new Vector3Int(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.y), Mathf.RoundToInt(v3.z));
    }

    /// <summary>
    /// Converts a Vector3 to a Vector3Int by cutting off the decimal value and only keeping the integer portion.
    /// </summary>
    /// <param name="v3">The Vector3 to convert to a Vector3Int.</param>
    /// <returns>Returns the integer portion of the given Vector3.</returns>
    public static Vector3Int ToInt(this Vector3 v3)
    {
        return new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);
    }

    /// <summary>
    /// Used to reorient a vector3 rotation to the orientation of a given transform.
    /// </summary>
    /// <param name="v3">The Vector3 to apply the reorientation to.</param>
    /// <param name="transform">The Transform to use for reorienting the given Vector3.</param>
    /// <returns>Returns the reoriented Vector3.</returns>
    public static Vector3 ReorientAlongTransform(this Vector3 v3, Transform transform)
    {
        return (v3.x * transform.right) + (v3.y * transform.up) + (v3.z * transform.forward);
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// Vector2Int
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets the manhattan distance from this Vector2Int to the given Vector2Int.
    /// </summary>
    /// <param name="from">The position to measure from.</param>
    /// <param name="to">The position to measure to.</param>
    /// <returns>Returns the manhattan distance (the absolute value of each component in "from" subtracted from "to" added together).</returns>
    public static int ManhattanDistance(this Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    /// <summary>
    /// Converts a Vector2Int to a normalized unit Vector2.
    /// </summary>
    /// <param name="v2">The Vector2Int to normalize.</param>
    /// <returns>Returns the normalized unit Vector2.</returns>
    public static Vector2 Normalize(this Vector2Int v2)
    {
        float sqr = Mathf.Sqrt((v2.x * v2.x) + (v2.y * v2.y));
        return new Vector2(v2.x / sqr, v2.y / sqr);
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// float
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Remaps a float input from one range of values to another range of values.
    /// </summary>
    /// <param name="input">The input to modify.</param>
    /// <param name="inMin">The minimum value of the input value's range.</param>
    /// <param name="inMax">The maximum value of the input value's range.</param>
    /// <param name="outMin">The minimum value of the output value's range.</param>
    /// <param name="outMax">The maximum value of the output value's range.</param>
    /// <returns>Returns the remapped value within the given output range.</returns>
    public static float Remap(this float input, float inMin, float inMax, float outMin, float outMax)
    {
        float slope = (outMax - outMin) / (inMax - inMin);
        float intercept = outMin - (slope * inMin);
        return (slope * input) + intercept;
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// Vector2IntSaveData
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Vector2IntSaveData to a Vector2Int.
    /// </summary>
    /// <param name="v2SO">The Vector2IntSaveData to convert to a Vector2Int.</param>
    /// <returns>Returns a Vector2Int.</returns>
    public static Vector2Int ToVector2Int(this Vector2IntSaveData v2SO)
    {
        return new Vector2Int(v2SO.x, v2SO.y);
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// Vector3IntSaveData
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a Vector3IntSaveData to a Vector3Int.
    /// </summary>
    /// <param name="v3SO">The Vector3IntSaveData to convert to a Vector3Int.</param>
    /// <returns>Returns a Vector3Int.</returns>
    public static Vector3Int ToVector3Int(this Vector3IntSaveData v3SO)
    {
        return new Vector3Int(v3SO.x, v3SO.y, v3SO.z);
    }

    /////////////////////////////////////////////////////////////////////////////////
    /// Generic Arrays
    /////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a three dimensional array to a single dimensional array with a length equal to the cube of the three dimensional array length of a single dimension.
    /// </summary>
    /// <typeparam name="T">They type of array.</typeparam>
    /// <param name="singleArray">The three dimensional array to convert to a single dimensional array.</param>
    /// <returns>Returns a single dimensional array equivalent to the given three dimensional array.</returns>
    public static T[] ToSingleArray<T>(this T[,,] multiArray)
    {
        int length = multiArray.GetLength(0);
        T[] singleArray = new T[length * length * length];
        for(int x = 0; x < length; x++)
        {
            for(int y = 0; y < length; y++)
            {
                for(int z = 0; z < length; z++)
                {
                    singleArray[z + (y * length) + (x * length * length)] = multiArray[x, y, z];
                }
            }
        }
        return singleArray;
    }

    /// <summary>
    /// Converts a single array to a three dimensional array with a length of each dimension equal to the cube root of the single array length.
    /// </summary>
    /// <typeparam name="T">They type of array.</typeparam>
    /// <param name="singleArray">The single array to convert to a three dimensional array.</param>
    /// <returns>Returns a three dimensional array equivalent to the given single dimensional array.</returns>
    public static T[,,] To3DArray<T>(this T[] singleArray)
    {
        int length = Mathf.RoundToInt(Mathf.Pow(singleArray.Length, 1f / 3f));
        T[,,] multiArray = new T[length, length, length];
        for(int x = 0; x < length; x++)
        {
            for(int y = 0; y < length; y++)
            {
                for(int z = 0; z < length; z++)
                {
                    multiArray[x, y, z] = singleArray[z + (y * length) + (x * length * length)];
                }
            }
        }
        return multiArray;
    }
}
