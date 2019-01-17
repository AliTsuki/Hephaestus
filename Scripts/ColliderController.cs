using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderController
{
    public ColliderController() { }

    BoxCollider[] colliders;
    Vector3 position;

    public void CreateGameObjectPool()
    {
        int index = 0;

        GameObject colliderParent = new GameObject("Collider Parent");

        colliders = new BoxCollider[Chunk.size.x * Chunk.size.y * Chunk.size.z];

        for(int i = 0; i < Chunk.size.x; i++)
        {
            for(int j = 0; j < Chunk.size.y; j++)
            {
                for(int k = 0; k < Chunk.size.z; k++)
                {
                    GameObject go = new GameObject();
                    colliders[index] = go.AddComponent<BoxCollider>();

                    position.x = i + 0.5f;
                    position.y = j + 0.5f;
                    position.z = k + 0.5f;

                    colliders[index].transform.position = position;

                    go.transform.SetParent(colliderParent.transform);
                    go.SetActive(false);

                    index++;
                }
            }
        }
    }

    public IEnumerator SetCollidersAtChunk(int x, int y, int z)
    {
        Chunk chunk = null;

        yield return new WaitUntil(() => World.instance.GetChunkAt(x, y, z, out chunk));
        yield return new WaitUntil(() => chunk.ready);

        int index = 0;

        for (int i = 0; i < Chunk.size.x; i++)
        {
            for (int j = 0; j < Chunk.size.y; j++)
            {
                for (int k = 0; k < Chunk.size.z; k++)
                {
                    if (chunk.faces[index] != 0)
                    {
                        colliders[index].gameObject.SetActive(true);

                        position.x = i + chunk.position.x + 0.5f;
                        position.y = i + chunk.position.y + 0.5f;
                        position.z = i + chunk.position.z + 0.5f;

                        colliders[index].transform.position = position;
                    }
                    else
                        colliders[index].gameObject.SetActive(false);
                    index++;
                }
            }
        }
    }

    public IEnumerator EditCollider(int x, int y, int z, Chunk chunk, bool mode)
    {
        yield return new WaitUntil(() => chunk.ready);

        int index = PointToIndex(x, y, z);
        int max = Chunk.size.x * Chunk.size.y * Chunk.size.z;

        if (mode == false)
        {
            colliders[index].gameObject.SetActive(false);

            index = PointToIndex(x, y, z + 1);
            if (index < max && index >= 0 && chunk.faces[index] != 0)
                colliders[index].gameObject.SetActive(true);
            else if(index < max)
                colliders[index].gameObject.SetActive(false);

            index = PointToIndex(x, y, z - 1);
            if (index < max && index >= 0 && chunk.faces[index] != 0)
                colliders[index].gameObject.SetActive(true);
            else if(index >= 0)
                colliders[index].gameObject.SetActive(false);

            index = PointToIndex(x, y + 1, z);
            if (index < max && index >= 0 && chunk.faces[index] != 0)
                colliders[index].gameObject.SetActive(true);
            else if (index < max)
                colliders[index].gameObject.SetActive(false);

            index = PointToIndex(x, y - 1, z);
            if (index < max && index >= 0 && chunk.faces[index] != 0)
                colliders[index].gameObject.SetActive(true);
            else if (index >= 0)
                colliders[index].gameObject.SetActive(false);

            index = PointToIndex(x + 1, y, z);
            if (index < max && index >= 0 && chunk.faces[index] != 0)
                colliders[index].gameObject.SetActive(true);
            else if (index < max)
                colliders[index].gameObject.SetActive(false);

            index = PointToIndex(x - 1, y, z);
            if (index < max && index >= 0 && chunk.faces[index] != 0)
                colliders[index].gameObject.SetActive(true);
            else if (index >= 0)
                colliders[index].gameObject.SetActive(false);
        }

        if (mode == true)
        {
            colliders[index].gameObject.SetActive(true);

            index = PointToIndex(x, y, z + 1);
            if (index < max && index >= 0 && chunk.faces[index] == 0)
                colliders[index].gameObject.SetActive(false);
            else if (index >= 0)
                colliders[index].gameObject.SetActive(true);

            index = PointToIndex(x, y, z - 1);
            if (index < max && index >= 0 && chunk.faces[index] == 0)
                colliders[index].gameObject.SetActive(false);
            else if (index < max)
                colliders[index].gameObject.SetActive(true);

            index = PointToIndex(x, y + 1, z);
            if (index < max && index >= 0 && chunk.faces[index] == 0)
                colliders[index].gameObject.SetActive(false);
            else if (index >= 0)
                colliders[index].gameObject.SetActive(true);

            index = PointToIndex(x, y - 1, z);
            if (index < max && index >= 0 && chunk.faces[index] == 0)
                colliders[index].gameObject.SetActive(false);
            else if (index < max)
                colliders[index].gameObject.SetActive(true);

            index = PointToIndex(x + 1, y, z);
            if (index < max && index >= 0 && chunk.faces[index] == 0)
                colliders[index].gameObject.SetActive(false);
            else if (index >= 0)
                colliders[index].gameObject.SetActive(true);

            index = PointToIndex(x - 1, y, z);
            if (index < max && index >= 0 && chunk.faces[index] == 0)
                colliders[index].gameObject.SetActive(false);
            else if (index < max)
                colliders[index].gameObject.SetActive(true);
        }
    }

    static int PointToIndex(int x, int y, int z)
    {
        if (IsPointWithinBounds(x, y, z) == false)
            return -1;

        return (x * Chunk.size.z * Chunk.size.y) + (y * Chunk.size.z) + z;
    }

    static bool IsPointWithinBounds(int x, int y, int z)
    {
        return x >= 0 && x < Chunk.size.x && y >= 0 && y < Chunk.size.y && z >= 0 && z < Chunk.size.z;
    }
}
