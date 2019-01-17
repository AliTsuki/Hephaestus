using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    public static World instance;
    public static Matrix4x4 id = Matrix4x4.identity;

    Dictionary<Vector3Int, Chunk> chunkPosMap;

    public Material material;
    public Texture texture;

    public int chunksTall;
    public int blocksTall;

    public World(int blocksTall, Material material)
    {
        instance = this;

        this.material = material;
        this.blocksTall = blocksTall;

        chunksTall = blocksTall / Chunk.size.y;

        chunkPosMap = new Dictionary<Vector3Int, Chunk>();
    }

    public bool CreateChunkDataAt(int x, int y, int z, out Chunk chunk)
    {
        Vector3Int pos = WorldCoordsToChunkCoords(x, y, z);

        if(chunkPosMap.ContainsKey(pos) == false)
        {
            chunk = new Chunk(pos);
            chunk.GenerateBlockArray();

            chunkPosMap.Add(pos, chunk);

            return true;
        }

        chunk = null;
        return false;
    }

    public bool RemoveChunkDataAt(int x, int y, int z)
    {
        Vector3Int key = WorldCoordsToChunkCoords(x, y, z);

        return chunkPosMap.Remove(key);
    }

    public IEnumerator GenerateChunkMeshAt(Chunk chunk)
    {
        GameController.instance.StartCoroutine(chunk.GenerateMesh());
        yield return new WaitUntil(() => chunk.ready);
    }

    public bool GetChunkAt(int x, int y, int z, out Chunk chunk)
    {
        Vector3Int key = WorldCoordsToChunkCoords(x, y, z);
        return chunkPosMap.TryGetValue(key, out chunk);
    }

    public bool GetChunkAt(Vector3 point, Vector3 normal, bool getAdjacent, out Chunk chunk)
    {
        Vector3Int key;

        if(getAdjacent)
        {
            key = WorldCoordsToChunkCoords(point.x + normal.x, point.y + normal.y, point.z + normal.z);
            return chunkPosMap.TryGetValue(key, out chunk);
        }

        key = WorldCoordsToChunkCoords(point.x, point.y, point.z);
        return chunkPosMap.TryGetValue(key, out chunk);
    }

    // Update is called once per frame
    public void Update()
    {
        foreach (Chunk ch in chunkPosMap.Values)
        {
            if (ch.mesh != null)
                Graphics.DrawMesh(ch.mesh, id, material, 0);
        }
    }

    public static Vector3Int WorldCoordsToChunkCoords(int x, int y, int z)
    {
        return new Vector3Int(
            Mathf.FloorToInt(x / (float)Chunk.size.x) * Chunk.size.x,
            Mathf.FloorToInt(y / (float)Chunk.size.y) * Chunk.size.y,
            Mathf.FloorToInt(z / (float)Chunk.size.z) * Chunk.size.z
            );
    }

    public static Vector3Int WorldCoordsToChunkCoords(float x, float y, float z)
    {
        return new Vector3Int(
            Mathf.FloorToInt(x / (float)Chunk.size.x) * Chunk.size.x,
            Mathf.FloorToInt(y / (float)Chunk.size.y) * Chunk.size.y,
            Mathf.FloorToInt(z / (float)Chunk.size.z) * Chunk.size.z
            );
    }
}
