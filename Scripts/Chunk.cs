using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static Vector3Int size = new Vector3Int(16, 256, 16);
    public Mesh mesh;
    public Vector3Int position;
    public bool ready = false;

    public Block[] blocks;
    public byte[] faces;

    public Chunk(Vector3Int pos) => position = pos;

    public void GenerateBlockArray()
    {
        blocks = new Block[size.x * size.y * size.z];
        int index = 0;

        int[,] height = new int[size.x, size.z];

        for(int x = 0; x < size.x; x++)
        {
            for(int z = 0; z < size.z; z++)
            {
                int value = Mathf.CeilToInt(
                    (Mathf.PerlinNoise((x + position.x + 84) / 32f, (z + position.z) / 32f) * 15f) +
                    (Mathf.PerlinNoise((x + position.x) / 64f, (z + position.z + 84) / 64f) * 27f) + 
                    (Mathf.PerlinNoise((x + position.x - 612) / 16f, (z+ position.z) / 16f) * 5f) +
                    Mathf.PerlinNoise((x + position.x) / 4f, ((z + position.z) / 4f) + 64) +
                    (Mathf.PerlinNoise((x + position.x + 8) / 24f, ((z + position.z) / 24f) - 8) * 12f) +
                    (Mathf.PerlinNoise((x + position.x + 80) / 64f, ((z + position.z) / 64f) - 80) * 40f) +
                    (Mathf.PerlinNoise((x + position.x + 8) / 128f, ((z + position.z) / 128f) - 12) * 80f) +
                    64f
                    );
                height[x, z] = value;
            }
        }

        for(int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                for(int z = 0; z < size.z; z++)
                {
                    if(y + position.y > height[x, z])
                    {
                        if(y + position.y == height[x, z] + 7 && Random.Range(0, 487) == 1)
                        {
                            StructureGenerator.GenerateWoodenPlatform(position, x, y, z, blocks);
                        }
                        index++;
                        continue;
                    }

                    if(y + position.y == height[x, z])
                        blocks[index] = Block.Grass;

                    if(y + position.y < height[x, z] && y + position.y > height[x, z] - 3)
                        blocks[index] = Block.Dirt;

                    if(y + position.y <= height[x, z] - 3)
                        blocks[index] = Block.Stone;

                    index++;
                }
            }
        }
        StructureGenerator.GetWaitingBlocks(position, blocks);
    }

    public IEnumerator GenerateMesh()
    {
        MeshBuilder builder = new MeshBuilder(position, blocks);
        builder.Start();

        yield return new WaitUntil(() => builder.Update());

        mesh = builder.GetMeshData(ref mesh);
        faces = builder.GetFaces(ref faces);

        ready = true;
        builder = null;
    }

    public Block GetBlockAt(int x, int y, int z)
    {
        x -= position.x;
        y -= position.y;
        z -= position.z;

        if(IsPointWithinBounds(x, y, z))
            return blocks[(x * size.y * size.z) + (y * size.z) + z];

        return Block.Air;
    }

    public bool SetBlockAt(Vector3 point, Vector3 normal, Block block, bool setBlockMode = false)
    {
        if(ready == false)
            return false;

        if(setBlockMode == false)
        {
            int x = Mathf.FloorToInt(point.x) - position.x;
            int y = Mathf.FloorToInt(point.y) - position.y;
            int z = Mathf.FloorToInt(point.z) - position.z;

            if (normal.x > 0.5)
                y -= 1;
            if (normal.y > 0.5)
                y -= 1;
            if (normal.z > 0.5)
                y -= 1;

            if (IsPointWithinBounds(x, y, z))
            {
                blocks[(x * Chunk.size.y * Chunk.size.z) + (y * Chunk.size.z) + z] = block;
                ready = false;
                GameController.instance.StartCoroutine(GenerateMesh());
                GameController.instance.EditCollider(x, y, z, this, setBlockMode);

                Chunk neighbor;
                if(x == 0 && World.instance.GetChunkAt(position.x - 1, position.y, position.z, out neighbor))
                {
                    neighbor.ready = false;
                    GameController.instance.StartCoroutine(neighbor.GenerateMesh());
                }
                if (x == Chunk.size.x - 1 && World.instance.GetChunkAt(position.x + Chunk.size.x, position.y, position.z, out neighbor))
                {
                    neighbor.ready = false;
                    GameController.instance.StartCoroutine(neighbor.GenerateMesh());
                }
                if (z == 0 && World.instance.GetChunkAt(position.x, position.y, position.z - 1, out neighbor))
                {
                    neighbor.ready = false;
                    GameController.instance.StartCoroutine(neighbor.GenerateMesh());
                }
                if (z == Chunk.size.z - 1 && World.instance.GetChunkAt(position.x, position.y, position.z + Chunk.size.z, out neighbor))
                {
                    neighbor.ready = false;
                    GameController.instance.StartCoroutine(neighbor.GenerateMesh());
                }

                return true;
            }
        }
        if(setBlockMode == true)
        {
            int x = Mathf.FloorToInt(point.x) - position.x;
            int y = Mathf.FloorToInt(point.y) - position.y;
            int z = Mathf.FloorToInt(point.z) - position.z;

            if (normal.x < -0.5)
                y -= 1;
            if (normal.y < -0.5)
                y -= 1;
            if (normal.z < -0.5)
                y -= 1;

            if (IsPointWithinBounds(x, y, z))
            {
                blocks[(x * Chunk.size.y * Chunk.size.z) + (y * Chunk.size.z) + z] = block;
                ready = false;
                GameController.instance.StartCoroutine(GenerateMesh());
                GameController.instance.EditCollider(x, y, z, this, setBlockMode);

                return true;
            }
        }

        return false;
    }

    bool IsPointWithinBounds(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 && z < Chunk.size.z && y < Chunk.size.y && x < Chunk.size.x;
    }
}
