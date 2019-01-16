using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static Vector3Int size = new Vector3Int(16, 32, 16);
    public Mesh mesh;
    public Vector3Int position;
    public bool ready = false;

    Block[] blocks;

    public Chunk(Vector3Int pos)
    {
        position = pos;
    }

    public void GenerateBlockArray()
    {
        blocks = new Block[size.x * size.y * size.z];
        int index = 0;
        
        for(int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                for(int z = 0; z < size.z; z++)
                {
                    if(Mathf.PerlinNoise(x / 32f, z / 32f) * 5f + 20f < y)
                        blocks[index] = Block.Filled;
                    index++;
                }
            }
        }
    }

    public IEnumerator GenerateMesh()
    {
        MeshBuilder builder = new MeshBuilder(position, blocks);
        builder.Start();

        yield return new WaitUntil(() => builder.Update());

        mesh = builder.GetMesh(ref mesh);
        ready = true;

        builder = null;
    }
}
